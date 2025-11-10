using Homework_portal.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Homework_portal.Models.ViewModels
{
    public class DersVM
    {
        public Ders Ders { get; set; }
        public SelectList? OgretmenListesi { get; set; }
    }
}