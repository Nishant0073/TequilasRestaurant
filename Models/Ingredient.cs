using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TequilasRestaurant.Models
{
    public class Ingredient
    {
        public int IngredientId { get; set; }
        public string? Name { get; set; }
        public string?  Description { get; set; }
        [ValidateNever]
        public List<ProductIngredient> ProductIngredients { get; set; }

    }
}