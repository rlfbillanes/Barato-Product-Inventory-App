namespace BaratoInventory.Blazor.Services;

public interface IAppModalService
{
    event Action? OnOpenAddProductRequested;
    event Action? OnFocusSearchRequested;

    void RequestOpenAddProduct();
    void RequestFocusSearch();
}

public class AppModalService : IAppModalService
{
    public event Action? OnOpenAddProductRequested;
    public event Action? OnFocusSearchRequested;

    public void RequestOpenAddProduct()
    {
        OnOpenAddProductRequested?.Invoke();
    }

    public void RequestFocusSearch()
    {
        OnFocusSearchRequested?.Invoke();
    }
}
