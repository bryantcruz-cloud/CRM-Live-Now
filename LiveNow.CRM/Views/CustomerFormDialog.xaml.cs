using System.Text.RegularExpressions;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Enums;
using Microsoft.UI.Xaml.Controls;

namespace LiveNow.CRM.Views;

public sealed partial class CustomerFormDialog : ContentDialog
{
    private CustomerDto? _existingCustomer;

    public CustomerFormDialog()
    {
        this.InitializeComponent();
        Title = "Nuevo cliente";
        _existingCustomer = null;
    }

    public CustomerFormDialog(CustomerDto customer)
    {
        this.InitializeComponent();
        Title = "Editar cliente";
        _existingCustomer = customer;
        LoadCustomerData(customer);
    }

    private void LoadCustomerData(CustomerDto customer)
    {
        FirstNameTextBox.Text = customer.FirstName;
        LastNameTextBox.Text = customer.LastName;
        EmailTextBox.Text = customer.Email;
        PhoneTextBox.Text = customer.Phone ?? string.Empty;
        CountryTextBox.Text = customer.Country ?? string.Empty;
        CityTextBox.Text = customer.City ?? string.Empty;
        NotesTextBox.Text = customer.Notes ?? string.Empty;

        if (customer.DateOfBirth.HasValue)
        {
            DateOfBirthDatePicker.Date = new DateTimeOffset(customer.DateOfBirth.Value);
        }

        StatusComboBox.SelectedIndex = (int)customer.Status - 1;
    }

    protected override void OnContentDialogButtonClick(ContentDialogButtonClickEventArgs args)
    {
        if (args.Button == ContentDialogButton.Primary)
        {
            if (!ValidateForm())
            {
                args.Cancel = true;
                return;
            }
        }
        base.OnContentDialogButtonClick(args);
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
        {
            ShowValidationError("El nombre es obligatorio.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
        {
            ShowValidationError("El apellido es obligatorio.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(EmailTextBox.Text) && !IsValidEmail(EmailTextBox.Text))
        {
            ShowValidationError("El formato del email no es válido.");
            return false;
        }

        ValidationInfoBar.IsOpen = false;
        return true;
    }

    private void ShowValidationError(string message)
    {
        ValidationInfoBar.Message = message;
        ValidationInfoBar.IsOpen = true;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public CreateCustomerDto? GetCreateDto()
    {
        if (!ValidateForm()) return null;

        return new CreateCustomerDto
        {
            FirstName = FirstNameTextBox.Text.Trim(),
            LastName = LastNameTextBox.Text.Trim(),
            Email = EmailTextBox.Text.Trim(),
            Phone = string.IsNullOrWhiteSpace(PhoneTextBox.Text) ? null : PhoneTextBox.Text.Trim(),
            Country = string.IsNullOrWhiteSpace(CountryTextBox.Text) ? null : CountryTextBox.Text.Trim(),
            City = string.IsNullOrWhiteSpace(CityTextBox.Text) ? null : CityTextBox.Text.Trim(),
            DateOfBirth = DateOfBirthDatePicker.Date?.DateTime,
            Notes = string.IsNullOrWhiteSpace(NotesTextBox.Text) ? null : NotesTextBox.Text.Trim()
        };
    }

    public UpdateCustomerDto? GetUpdateDto()
    {
        if (!ValidateForm()) return null;

        return new UpdateCustomerDto
        {
            FirstName = FirstNameTextBox.Text.Trim(),
            LastName = LastNameTextBox.Text.Trim(),
            Email = EmailTextBox.Text.Trim(),
            Phone = string.IsNullOrWhiteSpace(PhoneTextBox.Text) ? null : PhoneTextBox.Text.Trim(),
            Country = string.IsNullOrWhiteSpace(CountryTextBox.Text) ? null : CountryTextBox.Text.Trim(),
            City = string.IsNullOrWhiteSpace(CityTextBox.Text) ? null : CityTextBox.Text.Trim(),
            DateOfBirth = DateOfBirthDatePicker.Date?.DateTime,
            Notes = string.IsNullOrWhiteSpace(NotesTextBox.Text) ? null : NotesTextBox.Text.Trim(),
            Status = (CustomerStatusEnum)(StatusComboBox.SelectedIndex + 1)
        };
    }
}
