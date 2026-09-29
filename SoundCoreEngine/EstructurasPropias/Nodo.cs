namespace SoundCoreEngine.EstructurasPropias;

public sealed class Nodo<T>
{
    public Nodo(T valor) => Valor = valor;
    public T Valor { get; set; }
    public Nodo<T>? Siguiente { get; set; }
}
