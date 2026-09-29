using System.Diagnostics;
using SoundCoreEngine.EstructurasPropias;
using SoundCoreEngine.Modelos;
using SoundCoreEngine.Servicios;

namespace SoundCoreEngine;

public sealed class MainForm : Form
{
	private readonly ListaSimpleEnlazada<Pista> _colaPropia = new();
	private readonly LinkedList<Pista> _colaEnlazada = new();
	private readonly List<Pista> _colaLista = [];
	private readonly ReproductorVlc _reproductor = new();
	private readonly TextBox _txtTitulo = new() { PlaceholderText = "Título" };
	private readonly TextBox _txtArtista = new() { PlaceholderText = "Artista" };
	private readonly NumericUpDown _numBpm = new() { Minimum = 60, Maximum = 220, Value = 120, Width = 70 };
	private readonly NumericUpDown _numDuracion = new() { Minimum = 0, Maximum = 36000, Value = 180, Width = 80 };
	private readonly RadioButton _rbPropia = new() { Text = "Lista simple propia (nodos)", Checked = true, AutoSize = true };
	private readonly RadioButton _rbEnlazada = new() { Text = ".NET LinkedList<T>", AutoSize = true };
	private readonly RadioButton _rbLista = new() { Text = ".NET List<T>", AutoSize = true };
	private readonly DataGridView _grid = new();
	private readonly Label _lblArchivo = new() { Text = "Ningún archivo seleccionado", AutoEllipsis = true, Dock = DockStyle.Fill };
	private readonly Label _lblSonando = new() { Text = "▶ En espera", AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.DarkCyan };
	private readonly Label _lblEstadisticas = new() { AutoSize = true };
	private readonly TextBox _txtResultados = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 30, 45), ForeColor = Color.White, Font = new Font("Consolas", 9) };
	private readonly NumericUpDown _numPrueba = new() { Minimum = 1000, Maximum = 100000, Increment = 1000, Value = 20000, Width = 95 };
	private string? _archivoSeleccionado;
	private Pista? _pistaSonando;
	private int _siguienteId = 1;

	public MainForm()
	{
		Text = "SoundCore Engine — DJ Queue & VLC Player";
		MinimumSize = new Size(1050, 700);
		Size = new Size(1200, 780);
		StartPosition = FormStartPosition.CenterScreen;
		BackColor = Color.FromArgb(241, 245, 249);
		ConstruirInterfaz();
		_rbPropia.CheckedChanged += (_, _) => RefrescarVista();
		_rbEnlazada.CheckedChanged += (_, _) => RefrescarVista();
		_rbLista.CheckedChanged += (_, _) => RefrescarVista();
		_reproductor.Termino += (_, _) => BeginInvoke(AvanzarYReproducir);
		_reproductor.Error += (_, mensaje) => BeginInvoke(() => MessageBox.Show(mensaje, "Formato no compatible", MessageBoxButtons.OK, MessageBoxIcon.Warning));
		FormClosed += (_, _) => _reproductor.Dispose();
		RefrescarVista();
	}

	private void ConstruirInterfaz()
	{
		var raiz = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), RowCount = 3, ColumnCount = 1 };
		raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
		raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
		raiz.Controls.Add(CrearRegistro(), 0, 0);
		raiz.Controls.Add(CrearZonaCola(), 0, 1);
		raiz.Controls.Add(CrearBenchmark(), 0, 2);
		Controls.Add(raiz);
	}

    private Control CrearRegistro()
    {
        var caja = Caja("REGISTRO DE ARCHIVO / PISTA");
        caja.MinimumSize = new Size(0, 130);
        var fila = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Padding = new Padding(8) };
		var btnArchivo = Boton("📁 Elegir cualquier archivo", (_, _) => ElegirArchivo());
        fila.Controls.AddRange([btnArchivo, _lblArchivo, Etiqueta("Título:"), _txtTitulo, Etiqueta("Artista:"), _txtArtista,
            Etiqueta("BPM:"), _numBpm, Etiqueta("Duración (s):"), _numDuracion]);
        fila.Controls.Add(Etiqueta("Modo de estructura:"));
        fila.Controls.AddRange([_rbPropia, _rbEnlazada, _rbLista]);
        caja.Controls.Add(fila);
        return caja;
	}

	private Control CrearZonaCola()
	{
		var division = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 255, Panel1MinSize = 220 };
		var acciones = Caja("ACCIONES DE COLA");
		var botones = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(12) };
		botones.Controls.AddRange([
			Boton("+ Encolar al final", (_, _) => Encolar(false)),
			Boton("⏭ Reproducir siguiente", (_, _) => Encolar(true)),
			Boton("▶ Avanzar y reproducir", (_, _) => AvanzarYReproducir()),
			Boton("⏸ Pausar / reanudar", (_, _) => _reproductor.PausarOReanudar()),
			Boton("⇅ Invertir lista", (_, _) => Invertir()),
			Boton("⚡ Ordenar por BPM", (_, _) => Ordenar()),
			Boton("🧹 Purgar duplicados", (_, _) => Purgar())]);
		acciones.Controls.Add(botones);
		division.Panel1.Controls.Add(acciones);

		var cola = Caja("PLAYLIST VISUAL / COLA EN VIVO");
		var superior = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
		superior.Controls.Add(_lblSonando);
		cola.Controls.Add(CrearPieCola());
		cola.Controls.Add(_grid);
		cola.Controls.Add(superior);
		ConfigurarGrid();
		division.Panel2.Controls.Add(cola);
		return division;
	}

	private Control CrearPieCola()
	{
		var pie = new Panel { Dock = DockStyle.Bottom, Height = 34, Padding = new Padding(8) };
		pie.Controls.Add(_lblEstadisticas);
		return pie;
	}

	private Control CrearBenchmark()
	{
		var caja = Caja("BENCHMARK Y TELEMETRÍA");
		var cabecera = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
		cabecera.Controls.AddRange([Etiqueta("Pistas para prueba:"), _numPrueba, Boton("🚀 Iniciar prueba", (_, _) => EjecutarBenchmark())]);
		caja.Controls.Add(_txtResultados);
		caja.Controls.Add(cabecera);
		return caja;
	}

	private void ConfigurarGrid()
	{
		_grid.Dock = DockStyle.Fill;
		_grid.AllowUserToAddRows = false;
		_grid.AllowUserToDeleteRows = false;
		_grid.ReadOnly = true;
		_grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
		_grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
		_grid.BackgroundColor = Color.White;
		_grid.ColumnCount = 6;
		string[] nombres = ["Pos.", "ID", "Título / artista", "BPM", "Duración", "Formato"];
		for (var i = 0; i < nombres.Length; i++) _grid.Columns[i].Name = nombres[i];
	}

	private void ElegirArchivo()
	{
		using var dialogo = new OpenFileDialog { Filter = "Todos los archivos (*.*)|*.*", Title = "Selecciona un archivo para VLC" };
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;
        _archivoSeleccionado = dialogo.FileName;
        _lblArchivo.Text = _archivoSeleccionado;
        // Cada selección representa una nueva pista: actualizar el título evita reutilizar el anterior.
        _txtTitulo.Text = Path.GetFileNameWithoutExtension(_archivoSeleccionado);
        if (string.IsNullOrWhiteSpace(_txtArtista.Text)) _txtArtista.Text = "Artista desconocido";
	}

	private Pista CrearPista()
	{
		if (string.IsNullOrWhiteSpace(_archivoSeleccionado)) throw new InvalidOperationException("Primero selecciona un archivo. El selector acepta cualquier extensión.");
		return new Pista(_siguienteId++,
			string.IsNullOrWhiteSpace(_txtTitulo.Text) ? Path.GetFileNameWithoutExtension(_archivoSeleccionado) : _txtTitulo.Text.Trim(),
			string.IsNullOrWhiteSpace(_txtArtista.Text) ? "Artista desconocido" : _txtArtista.Text.Trim(),
			(int)_numBpm.Value, (int)_numDuracion.Value, _archivoSeleccionado);
	}

	private void Encolar(bool siguiente)
	{
		try
		{
			var pista = CrearPista();
			if (_rbPropia.Checked) { if (siguiente) _colaPropia.ReproducirSiguiente(pista); else _colaPropia.AgregarAlFinal(pista); }
			else if (_rbEnlazada.Checked)
			{
				if (siguiente && _colaEnlazada.First is not null) _colaEnlazada.AddAfter(_colaEnlazada.First, pista);
				else if (siguiente) _colaEnlazada.AddFirst(pista); else _colaEnlazada.AddLast(pista);
			}
			else { if (siguiente && _colaLista.Count > 0) _colaLista.Insert(1, pista); else _colaLista.Add(pista); }
			RefrescarVista();
		}
		catch (InvalidOperationException ex) { Aviso(ex.Message); }
	}

	private void AvanzarYReproducir()
	{
		try
		{
			_pistaSonando = SacarCabeza();
			_reproductor.Reproducir(_pistaSonando);
			_lblSonando.Text = $"▶ Sonando: {_pistaSonando.Titulo} — {_pistaSonando.Artista} [{_pistaSonando.Formato}]";
			RefrescarVista();
		}
		catch (InvalidOperationException) { Aviso("No hay pistas pendientes en la cola seleccionada."); }
		catch (FileNotFoundException ex) { Aviso(ex.Message); RefrescarVista(); }
	}

	private Pista SacarCabeza()
	{
		if (_rbPropia.Checked) return _colaPropia.AvanzarPista();
		if (_rbEnlazada.Checked)
		{
			if (_colaEnlazada.First is null) throw new InvalidOperationException();
			var pista = _colaEnlazada.First.Value; _colaEnlazada.RemoveFirst(); return pista;
		}
		if (_colaLista.Count == 0) throw new InvalidOperationException();
		var resultado = _colaLista[0]; _colaLista.RemoveAt(0); return resultado;
	}

	private void Invertir()
	{
		if (_rbPropia.Checked) _colaPropia.Invertir();
		else if (_rbEnlazada.Checked) { var copia = _colaEnlazada.Reverse().ToList(); _colaEnlazada.Clear(); foreach (var p in copia) _colaEnlazada.AddLast(p); }
		else _colaLista.Reverse();
		RefrescarVista();
	}

	private void Ordenar()
	{
		if (_rbPropia.Checked) _colaPropia.OrdenarEnSitio((a, b) => a.Bpm.CompareTo(b.Bpm));
		else if (_rbEnlazada.Checked) { var orden = _colaEnlazada.OrderBy(p => p.Bpm).ToList(); _colaEnlazada.Clear(); foreach (var p in orden) _colaEnlazada.AddLast(p); }
		else _colaLista.Sort((a, b) => a.Bpm.CompareTo(b.Bpm));
		RefrescarVista();
	}

	private void Purgar()
	{
		if (_rbPropia.Checked) _colaPropia.DepurarDuplicados((a, b) => string.Equals(a.Titulo, b.Titulo, StringComparison.OrdinalIgnoreCase));
		else if (_rbEnlazada.Checked) { var unicos = _colaEnlazada.DistinctBy(p => p.Titulo, StringComparer.OrdinalIgnoreCase).ToList(); _colaEnlazada.Clear(); foreach (var p in unicos) _colaEnlazada.AddLast(p); }
		else { var unicos = _colaLista.DistinctBy(p => p.Titulo, StringComparer.OrdinalIgnoreCase).ToList(); _colaLista.Clear(); _colaLista.AddRange(unicos); }
		RefrescarVista();
	}

	private IEnumerable<Pista> ColaActiva() => _rbPropia.Checked ? _colaPropia : _rbEnlazada.Checked ? _colaEnlazada : _colaLista;

	private void RefrescarVista()
	{
		_grid.Rows.Clear();
		var totalSegundos = 0; var posicion = 1;
		foreach (var p in ColaActiva()) { _grid.Rows.Add(posicion++, p.Id, $"{p.Titulo} — {p.Artista}", p.Bpm, FormatearTiempo(p.DuracionSegundos), p.Formato); totalSegundos += p.DuracionSegundos; }
		_lblEstadisticas.Text = $"Total en cola: {posicion - 1}  |  Duración estimada: {FormatearTiempo(totalSegundos)}";
	}

	private void EjecutarBenchmark()
	{
		var n = (int)_numPrueba.Value;
		var sw = new Stopwatch();
		var propia = new ListaSimpleEnlazada<int>(); propia.AgregarAlFinal(0);
		sw.Start(); for (var i = 1; i <= n; i++) propia.ReproducirSiguiente(i); sw.Stop(); var tPropia = sw.ElapsedMilliseconds;
		var enlazada = new LinkedList<int>(); enlazada.AddFirst(0); var nodo = enlazada.First!;
		sw.Restart(); for (var i = 1; i <= n; i++) enlazada.AddAfter(nodo, i); sw.Stop(); var tEnlazada = sw.ElapsedMilliseconds;
		var lista = new List<int> { 0 };
		sw.Restart(); for (var i = 1; i <= n; i++) lista.Insert(1, i); sw.Stop(); var tLista = sw.ElapsedMilliseconds;
		_txtResultados.Text = $"=== {n:N0} INSERCIONES INTERMEDIAS ==={Environment.NewLine}" +
			$"Lista simple propia: {tPropia,6} ms  | O(1): reconecta enlaces{Environment.NewLine}" +
			$".NET LinkedList<T>:  {tEnlazada,6} ms  | O(1): inserta con nodo conocido{Environment.NewLine}" +
			$".NET List<T>:        {tLista,6} ms  | O(n): desplaza elementos del arreglo{Environment.NewLine}{Environment.NewLine}" +
			"Conclusión: List<T> copia elementos al insertar en medio; las listas enlazadas cambian referencias.";
	}

	private static GroupBox Caja(string texto) => new() { Text = texto, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9, FontStyle.Bold), Padding = new Padding(8) };
	private static Label Etiqueta(string texto) => new() { Text = texto, AutoSize = true, Padding = new Padding(5, 6, 2, 2) };
	private static Button Boton(string texto, EventHandler evento)
	{
		var boton = new Button
		{
			Text = texto,
			AutoSize = true,
			MinimumSize = new Size(190, 34),
			FlatStyle = FlatStyle.Flat,
			BackColor = Color.White,
			Margin = new Padding(4)
		};
		boton.Click += evento;
		return boton;
	}
	private static string FormatearTiempo(int segundos) => TimeSpan.FromSeconds(segundos).ToString(segundos >= 3600 ? @"h\:mm\:ss" : @"mm\:ss");
	private void Aviso(string mensaje) => MessageBox.Show(mensaje, "SoundCore Engine", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
