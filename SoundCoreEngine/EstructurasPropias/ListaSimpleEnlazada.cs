using System.Collections;

namespace SoundCoreEngine.EstructurasPropias;

/// <summary>Lista enlazada genérica implementada únicamente mediante enlaces de Nodo&lt;T&gt;.</summary>
public sealed class ListaSimpleEnlazada<T> : IEnumerable<T>
{
    public Nodo<T>? Cabeza { get; private set; }
    public int Conteo { get; private set; }
    public bool EstaVacia => Cabeza is null;

    public void AgregarAlFinal(T valor)
    {
        var nuevo = new Nodo<T>(valor);
        if (Cabeza is null) { Cabeza = nuevo; Conteo++; return; }
        var actual = Cabeza;
        while (actual.Siguiente is not null) actual = actual.Siguiente;
        actual.Siguiente = nuevo;
        Conteo++;
    }

    public void ReproducirSiguiente(T valor)
    {
        var nuevo = new Nodo<T>(valor);
        if (Cabeza is null) Cabeza = nuevo;
        else { nuevo.Siguiente = Cabeza.Siguiente; Cabeza.Siguiente = nuevo; }
        Conteo++;
    }

    public T AvanzarPista()
    {
        if (Cabeza is null) throw new InvalidOperationException("La cola está vacía.");
        var valor = Cabeza.Valor;
        Cabeza = Cabeza.Siguiente;
        Conteo--;
        return valor;
    }

    // Inversión estrictamente in-place: solo se reorientan referencias Siguiente.
    public void Invertir()
    {
        Nodo<T>? previo = null;
        var actual = Cabeza;
        while (actual is not null)
        {
            var siguiente = actual.Siguiente;
            actual.Siguiente = previo;
            previo = actual;
            actual = siguiente;
        }
        Cabeza = previo;
    }

    public void InsertarOrdenado(T valor, Comparison<T> comparador)
    {
        var nuevo = new Nodo<T>(valor);
        if (Cabeza is null || comparador(valor, Cabeza.Valor) < 0)
        {
            nuevo.Siguiente = Cabeza;
            Cabeza = nuevo;
            Conteo++;
            return;
        }
        var actual = Cabeza;
        while (actual.Siguiente is not null && comparador(valor, actual.Siguiente.Valor) >= 0)
            actual = actual.Siguiente;
        nuevo.Siguiente = actual.Siguiente;
        actual.Siguiente = nuevo;
        Conteo++;
    }

    // Reordena nodos existentes sin crear otra lista ni cambiar sus valores.
    public void OrdenarEnSitio(Comparison<T> comparador)
    {
        Nodo<T>? ordenada = null;
        var actual = Cabeza;
        while (actual is not null)
        {
            var siguiente = actual.Siguiente;
            if (ordenada is null || comparador(actual.Valor, ordenada.Valor) < 0)
            {
                actual.Siguiente = ordenada;
                ordenada = actual;
            }
            else
            {
                var cursor = ordenada;
                while (cursor.Siguiente is not null && comparador(actual.Valor, cursor.Siguiente.Valor) >= 0)
                    cursor = cursor.Siguiente;
                actual.Siguiente = cursor.Siguiente;
                cursor.Siguiente = actual;
            }
            actual = siguiente;
        }
        Cabeza = ordenada;
    }

    public void DepurarDuplicados(Func<T, T, bool> sonIguales)
    {
        for (var actual = Cabeza; actual is not null; actual = actual.Siguiente)
        {
            var corredor = actual;
            while (corredor.Siguiente is not null)
            {
                if (sonIguales(actual.Valor, corredor.Siguiente.Valor))
                {
                    corredor.Siguiente = corredor.Siguiente.Siguiente;
                    Conteo--;
                }
                else corredor = corredor.Siguiente;
            }
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var actual = Cabeza; actual is not null; actual = actual.Siguiente) yield return actual.Valor;
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
