using System.ComponentModel.DataAnnotations;

namespace KolosMarcin.DTOs;

public class MedicamentDto
{
        
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(100)]
        public string Description { get; set; }
        [MaxLength(100)]
        public string Type { get; set; }
    
}