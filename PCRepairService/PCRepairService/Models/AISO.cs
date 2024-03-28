﻿using System.ComponentModel.DataAnnotations;

namespace PCRepairService.Models
{
    //All Inclusive Service Order -> simple datamodel with everything it needs
    public class AISO
    {
        public long Id { get; set; }
        public string ServiceOrderType { get; set; }
        public string? Description { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public bool IsCompleted { get; set; }
    }
}