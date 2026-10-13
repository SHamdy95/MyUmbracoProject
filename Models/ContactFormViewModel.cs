using System.ComponentModel.DataAnnotations;

namespace MyUmbracoProject.Models
{
    public class ContactFormViewModel
    {

     
            public string FirstName { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }
         public string Subject { get; set; }
        [Required(ErrorMessage = "Message is required.")]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
        public string Message { get; set; }
        
    }
}
