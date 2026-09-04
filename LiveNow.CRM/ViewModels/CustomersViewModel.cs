using System.Collections.ObjectModel;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Services;

namespace LiveNow.CRM.ViewModels;

/// <summary>
/// ViewModel for the Customers module.
/// Handles listing, searching, pagination, create, edit, and detail view.
/// </summary>
public class CustomersViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;

    private ObservableCollection<CustomerDto> _customers = new();
    private string _searchText = string.Empty;
    private int _currentPage = 1;
    private int _pageSize = 20;
    private int _totalCount;
    private int _totalPages;
    private CustomerDto? _selectedCustomer;
    private bool _isEditing;

    public CustomersViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public ObservableCollection<CustomerDto> Customers
    {
        get => _customers;
        private set => SetProperty(ref _customers, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                _currentPage = 1;
            }
        }
    }

    public int CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    public int PageSize
    {
        get => _pageSize;
        private set => SetProperty(ref _pageSize, value);
    }

    public int TotalCount
    {
        get => _totalCount;
        private set => SetProperty(ref _totalCount, value);
    }

    public int TotalPages
    {
        get => _totalPages;
        private set => SetProperty(ref _totalPages, value);
    }

    public CustomerDto? SelectedCustomer
    {
        get => _selectedCustomer;
        set => SetProperty(ref _selectedCustomer, value);
    }

    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public string PageInfo => TotalCount == 0
        ? "Sin resultados"
        : $"Página {CurrentPage} de {TotalPages} ({TotalCount} clientes)";

    public async Task LoadCustomersAsync()
    {
        ClearError();
        IsLoading = true;

        try
        {
            var result = await _apiClient.GetCustomersAsync(
                search: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                page: CurrentPage,
                pageSize: PageSize);

            if (result is not null)
            {
                Customers = new ObservableCollection<CustomerDto>(result.Items);
                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;
            }
            else
            {
                Customers = new ObservableCollection<CustomerDto>();
                TotalCount = 0;
                TotalPages = 0;
            }

            OnPropertyChanged(nameof(HasPreviousPage));
            OnPropertyChanged(nameof(HasNextPage));
            OnPropertyChanged(nameof(PageInfo));
        }
        catch (ApiException ex)
        {
            SetError($"Error al cargar clientes: {ex.Message}");
        }
        catch (Exception ex)
        {
            SetError($"Error inesperado: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadCustomersAsync();
    }

    public async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages) return;
        CurrentPage = page;
        await LoadCustomersAsync();
    }

    public async Task GoToFirstPageAsync() => await GoToPageAsync(1);
    public async Task GoToPreviousPageAsync() => await GoToPageAsync(CurrentPage - 1);
    public async Task GoToNextPageAsync() => await GoToPageAsync(CurrentPage + 1);
    public async Task GoToLastPageAsync() => await GoToPageAsync(TotalPages);

    public async Task<CustomerDto?> CreateCustomerAsync(CreateCustomerDto dto)
    {
        ClearError();
        IsLoading = true;

        try
        {
            var created = await _apiClient.CreateCustomerAsync(dto);
            return created;
        }
        catch (ApiException ex)
        {
            SetError($"Error al crear cliente: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            SetError($"Error inesperado: {ex.Message}");
            return null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<CustomerDto?> UpdateCustomerAsync(Guid id, UpdateCustomerDto dto)
    {
        ClearError();
        IsLoading = true;

        try
        {
            var updated = await _apiClient.UpdateCustomerAsync(id, dto);
            return updated;
        }
        catch (ApiException ex)
        {
            SetError($"Error al actualizar cliente: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            SetError($"Error inesperado: {ex.Message}");
            return null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> DeleteCustomerAsync(Guid id)
    {
        ClearError();
        IsLoading = true;

        try
        {
            await _apiClient.DeleteCustomerAsync(id);
            return true;
        }
        catch (ApiException ex)
        {
            SetError($"Error al eliminar cliente: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            SetError($"Error inesperado: {ex.Message}");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid id)
    {
        ClearError();
        try
        {
            return await _apiClient.GetCustomerAsync(id);
        }
        catch (ApiException ex)
        {
            SetError($"Error al obtener cliente: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            SetError($"Error inesperado: {ex.Message}");
            return null;
        }
    }
}
