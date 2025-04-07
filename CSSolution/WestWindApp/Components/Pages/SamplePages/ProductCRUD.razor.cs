using WestWindSystem.Entities;
using WestWindSystem.BLL;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WestWindApp.Components.Pages.SamplePages
{
    public partial class ProductCRUD
    {
        //to demo that code can be in EITHER this code-behind file
        //  or in the @code on the form file, the following field
        //  has been commented out here and activated in the @code 
        //  the form file

        //private string feedBackMessage = "";

        [Inject]
        private ProductServices _productServices { get; set; }
        private Product CurrentProduct = new();

        //declare the routing parameter
        //this property needs a special annotation to indicate it is a parameter
        //parameter properties need to be accessable by other pages (aka classes)
        //  therefore they need to be public
        [Parameter]
        public int? productid { get; set; }

        [Inject]
        private CategoryServices _categoryServices { get; set; }
        private List<Category> categories = new();

        // private int selectedCategoryId;
        [Inject]
        private SupplierServices _supplierServices { get; set; }
        private List<Supplier> suppliers = new();

        [Inject]
        protected NavigationManager CurrentNavigationManager { get; set; }

        [Inject]
        protected IJSRuntime jsRuntime { get; set; }

        //EditForm variables
        //declare the instance for your EditContext
        private EditContext editContext;
        //declare the ValidationMessageStore
        private ValidationMessageStore validationMessageStore;

        //this property is used to format the UnitPrice to 2 decimal places
        //it is within this property that the data is extracted and set to the
        //      actual property in the product instance
        //private decimal FormatUnitPrice
        //{
        //    get { return Math.Round(CurrentProduct.UnitPrice, 2); }
        //    set { CurrentProduct.UnitPrice = value; }
        //}

        protected override void OnInitialized()
        {
            //when the page is first render, we need to determind if the page was called with
            //   a pkey parameter value
            // No value: assumption is a NEW (create) product will be done, nothing to lookup
            // Yes value: assumption an existing product record will be altered or deleted
            //            the current record on the database should be displayed to the user
            //            within this method, lookup the record to display
            //since the productid is a nullable variable, add the .Value to obtain the value,
            //  and use .HasValue to test for the value

            //NOTE: the retrieval of the product instance is REQUIRED BEFORE assigning the instance
            //          to EditContext
            if (productid.HasValue)
                CurrentProduct = _productServices.Product_GetByID(productid.Value);

            //create the EditContext instance AND tie to the instance of the entity of the form
            editContext = new EditContext(CurrentProduct);

            //create vaidation MessageStore instance and indicate which EditContext it is 
            //      associated with
            //needed for custom validation within the event code and
            //   allows the user to use the ValidationMesssage controls of the form
            validationMessageStore = new ValidationMessageStore(editContext);



            categories = _categoryServices.Categories_Get();
            suppliers = _supplierServices.Supplier_GetList();
            base.OnInitialized();

        }

        private Exception GetInnerException(Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex;
        }

        private void OnCreate()
        {
            //clear old messages
            feedBackMessage = "";
            validationMessageStore.Clear(); //this replaces the old List<string> errorMsgs

            try
            {
                //the first test is to check to see if there are ANY errors already caught
                //  by the form using the DataAnnotationsValidator?
                if (editContext.Validate())
                {
                    //entering this code means that the DataAnnotation of your entity is satisfied

                    //if there are additional custom validation of your data
                    //  you can still do that within the event coding

                    //custom validation for this form will check to see if a category
                    //  and supplier have been select (why: the list have a prompt that
                    //  has a non valid sql foreign key value)

                    //Problem:
                    // CategoryID is an integer
                    // the default of an integer is 0 ( a value)
                    // NO foreign key/primary key on the database is 0
                    //      if a proper selection is not done, then when the
                    //      record is attempted to be added to the the database
                    //      the database will thrown an exception

                    //Solution: custom validation
                    if (CurrentProduct.CategoryID == 0)
                    {
                        //construction of the validation message store command
                        // .Add to your instance of the form for the indicated field
                        // parameter 1: the associated field to display the message
                        //              looking for the "field identifier"
                        //              use nameof(xxx) to supplier of the "field identifier"
                        //      example: editContext.Field(nameof(CurrentProduct.CategoryID))
                        // parameter 2: the message to display
                        //      example: "You must select a category"
                        validationMessageStore.Add(editContext.Field(nameof(CurrentProduct.CategoryID)),
                                                   "You must select a category");
                    }
                    if (CurrentProduct.SupplierID == 0)
                    {
                        validationMessageStore.Add(editContext.Field(nameof(CurrentProduct.SupplierID)),
                                                   "You must select a supplier");
                    }

                    //once your custom validation is done, if there was any new errors, you can check to see
                    //  what your validationMessageStore contains

                    //if there are any new messages , indicate to the form that the store has been altered
                    //to do this, we will use the Linq method .Any()  to determine if something exists
                    //NOTE: the .Any() returns only a true or false; it does NOT return an actual records
                    //      the .Where() returns actual record
                    if (editContext.GetValidationMessages().Any())
                    {
                        //notify  the editcontext that there has been a change to the message store
                        editContext.NotifyValidationStateChanged();
                    }
                    else
                    {
                        //the program assumes that the data is clean and can be further processed

                        //call the product services to receive the "good" data and add it to the database (CREATE!!!)
                        //call the appropriate service method and pass the current product instance
                        //the service method will return the new (identity) primary key value.
                        int newproductid = _productServices.Product_Add(CurrentProduct);

                        //if an error was thrown by the ProductServices or by the database
                        //  control will be passed to the catch
                        //if no error was thrown by the ProductServices or by the database
                        //  one assumes the data is on the database: success
                        //you need to communicate with the user this success
                        feedBackMessage = $"Product {CurrentProduct.ProductName} (id: {newproductid}) has been saved";
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                //usually if an error occurs only ONE message will be executed
                //You could setup a special error message string to display on your page
                //  that is NOT the validationMessageStore and is NOT the feedBackMessage
                //You could style this special error message string as an alert alert-danger
                feedBackMessage = $"Missing Data: {GetInnerException(ex).Message}";
            }
            catch (ArgumentException ex)
            {
                feedBackMessage = $"Data Issue: {GetInnerException(ex).Message}";
            }
            catch (Exception ex)
            {
                feedBackMessage = $"System Error: {GetInnerException(ex).Message}";
            }
        }

        private void OnUpdate()
        {
            //clear old messages
            feedBackMessage = "";
            validationMessageStore.Clear(); //this replaces the old List<string> errorMsgs

            try
            {
                //the first test is to check to see if there are ANY errors already caught
                //  by the form using the DataAnnotationsValidator?
                if (editContext.Validate())
                {

                    if (CurrentProduct.CategoryID == 0)
                    {
                        validationMessageStore.Add(editContext.Field(nameof(CurrentProduct.CategoryID)),
                                                   "You must select a category");
                    }
                    if (CurrentProduct.SupplierID == 0)
                    {
                        validationMessageStore.Add(editContext.Field(nameof(CurrentProduct.SupplierID)),
                                                   "You must select a supplier");
                    }

                    if (editContext.GetValidationMessages().Any())
                    {
                        //notify  the editcontext that there has been a change to the message store
                        editContext.NotifyValidationStateChanged();
                    }
                    else
                    {
                        //the program assumes that the data is clean and can be further processed

                        int rowsaffected = _productServices.Product_Update(CurrentProduct);

                        //Question: DID you ACTUALLY change any records?
                        //situation: an update was attempted but no records on the database were
                        //              actually change AND the action did NOT abort 
                        //in this case rowsafftected = 0
                        if (rowsaffected == 0)
                        {
                            feedBackMessage = $"Product {CurrentProduct.ProductName} (id: {CurrentProduct.ProductID}) has not been updated";

                        }
                        else
                        {
                            feedBackMessage = $"Product {CurrentProduct.ProductName} (id: {CurrentProduct.ProductID}) has been updated";

                        }
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                //usually if an error occurs only ONE message will be executed
                //You could setup a special error message string to display on your page
                //  that is NOT the validationMessageStore and is NOT the feedBackMessage
                //You could style this special error message string as an alert alert-danger
                feedBackMessage = $"Missing Data: {GetInnerException(ex).Message}";
            }
            catch (ArgumentException ex)
            {
                feedBackMessage = $"Data Issue: {GetInnerException(ex).Message}";
            }
            catch (Exception ex)
            {
                feedBackMessage = $"System Error: {GetInnerException(ex).Message}";
            }
        }

        private void OnDiscontinue()
        {
            //clear old messages
            feedBackMessage = "";
            validationMessageStore.Clear(); //this replaces the old List<string> errorMsgs

            try
            {
                //the first test is to check to see if there are ANY errors already caught
                //  by the form using the DataAnnotationsValidator?
                if (editContext.Validate())
                {

                    if (CurrentProduct.CategoryID == 0)
                    {
                        validationMessageStore.Add(editContext.Field(nameof(CurrentProduct.CategoryID)),
                                                   "You must select a category");
                    }
                    if (CurrentProduct.SupplierID == 0)
                    {
                        validationMessageStore.Add(editContext.Field(nameof(CurrentProduct.SupplierID)),
                                                   "You must select a supplier");
                    }

                    if (editContext.GetValidationMessages().Any())
                    {
                        //notify  the editcontext that there has been a change to the message store
                        editContext.NotifyValidationStateChanged();
                    }
                    else
                    {
                        //the program assumes that the data is clean and can be further processed

                        int rowsaffected = _productServices.Product_LogicalDelete(CurrentProduct);

                        //Question: DID you ACTUALLY change any records?
                        //situation: an update was attempted but no records on the database were
                        //              actually change AND the action did NOT abort
                        //in this case rowsafftected = 0
                        if (rowsaffected == 0)
                        {
                            feedBackMessage = $"Product {CurrentProduct.ProductName} (id: {CurrentProduct.ProductID}) has not been discontinued";

                        }
                        else
                        {
                            feedBackMessage = $"Product {CurrentProduct.ProductName} (id: {CurrentProduct.ProductID}) has been discontinued";

                        }
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                //usually if an error occurs only ONE message will be executed
                //You could setup a special error message string to display on your page
                //  that is NOT the validationMessageStore and is NOT the feedBackMessage
                //You could style this special error message string as an alert alert-danger
                feedBackMessage = $"Missing Data: {GetInnerException(ex).Message}";
            }
            catch (ArgumentException ex)
            {
                feedBackMessage = $"Data Issue: {GetInnerException(ex).Message}";
            }
            catch (Exception ex)
            {
                feedBackMessage = $"System Error: {GetInnerException(ex).Message}";
            }
        }

        private void OnActivate()
        {
            //clear old messages
            feedBackMessage = "";
            validationMessageStore.Clear(); //this replaces the old List<string> errorMsgs

            try
            {
                //the first test is to check to see if there are ANY errors already caught
                //  by the form using the DataAnnotationsValidator?
                if (editContext.Validate())
                {

                    if (CurrentProduct.CategoryID == 0)
                    {
                        validationMessageStore.Add(editContext.Field(nameof(CurrentProduct.CategoryID)),
                                                   "You must select a category");
                    }
                    if (CurrentProduct.SupplierID == 0)
                    {
                        validationMessageStore.Add(editContext.Field(nameof(CurrentProduct.SupplierID)),
                                                   "You must select a supplier");
                    }

                    if (editContext.GetValidationMessages().Any())
                    {
                        //notify  the editcontext that there has been a change to the message store
                        editContext.NotifyValidationStateChanged();
                    }
                    else
                    {
                        //the program assumes that the data is clean and can be further processed

                        int rowsaffected = _productServices.Product_Activation(CurrentProduct);

                        //Question: DID you ACTUALLY change any records?
                        //situation: an update was attempted but no records on the database were
                        //              actually change AND the action did NOT abort
                        //in this case rowsafftected = 0
                        if (rowsaffected == 0)
                        {
                            feedBackMessage = $"Product {CurrentProduct.ProductName} (id: {CurrentProduct.ProductID}) has not been activated";

                        }
                        else
                        {
                            feedBackMessage = $"Product {CurrentProduct.ProductName} (id: {CurrentProduct.ProductID}) has been activated";

                        }
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                //usually if an error occurs only ONE message will be executed
                //You could setup a special error message string to display on your page
                //  that is NOT the validationMessageStore and is NOT the feedBackMessage
                //You could style this special error message string as an alert alert-danger
                feedBackMessage = $"Missing Data: {GetInnerException(ex).Message}";
            }
            catch (ArgumentException ex)
            {
                feedBackMessage = $"Data Issue: {GetInnerException(ex).Message}";
            }
            catch (Exception ex)
            {
                feedBackMessage = $"System Error: {GetInnerException(ex).Message}";
            }

        }

        private async Task OnClear()
        {
            feedBackMessage = "";
            object[] messageline = new object[]
            {"Clearing the page will lose all unsaved data. Are you sure you wish to continue?"};
            if (await jsRuntime.InvokeAsync<bool>("confirm", messageline))
            {
                //remove any old custom error message place in the validation message store
                validationMessageStore.Clear();

                //clear the fields
                //clear the current product instance of any data (not using local variables)
                //notice that the "system" constructor is used as the entity has NO constructor code within it
                CurrentProduct = new Product();

                //IMPORTANT!!!!!!!!!!!!!!!!!!
                //the editContext is associated with the current product instance
                //the editContext REQUIRES to be reset to the NEW current product instance
                editContext = new EditContext(CurrentProduct);
            }
        }
        private async Task GoToSearch()
        {
            //clear all old messages
            feedBackMessage = "";
            object[] messageline = new object[]
            {"Leaving the page will lose all unsaved data. Are you sure you wish to continue?"};
            if (await jsRuntime.InvokeAsync<bool>("confirm", messageline))
            {
                CurrentNavigationManager.NavigateTo("categoryproducts");
            }
        }
    }
}
