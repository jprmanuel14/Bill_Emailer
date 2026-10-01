using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace BillColl_Main.CustomAttribute
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredIfTrue: RequiredAttribute, IClientModelValidator
    {
        private string PropertyName { get; set; }

        public RequiredIfTrue(string propertyName)
        {
            PropertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            object instance = context.ObjectInstance;
            Type type = instance.GetType();

            bool.TryParse(type.GetProperty(PropertyName).GetValue(instance)?.ToString(), out bool propertyValue);

            if (propertyValue && string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-required", ErrorMessage);
        }



        //public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ClientModelValidationContext context)
        //{
        //    //string errorMessage = this.FormatErrorMessage(metadata.DisplayName);
        //    string errorMessage = ErrorMessageString;

        //    // The value we set here are needed by the jQuery adapter
        //    ModelClientValidationRule isEngagementRule = new ModelClientValidationRule();


        //    isEngagementRule.ErrorMessage = errorMessage;
        //    isEngagementRule.ValidationType = "isengagementtrue"; // This is the name the jQuery adapter will use
        //    //"otherpropertyname" is the name of the jQuery parameter for the adapter, must be LOWERCASE!
        //    isEngagementRule.ValidationParameters.Add("propertyname", PropertyName);

        //    yield return isEngagementRule;
        //}


    }
}
