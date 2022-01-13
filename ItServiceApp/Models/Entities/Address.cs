﻿using ItServiceApp.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItServiceApp.Models.Entities
{
    public class Address :BaseEntity
    {
        public string Line { get; set; }
        public string PasCode { get; set; }
        public AddressTypes AddressType { get; set; }
        public int StateId { get; set; }

        [StringLength(450)]
        public string UserId { get; set; }

        [ForeignKey(nameof(StateId))]
        public virtual State State { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }

    }

    public enum AddressTypes
    {
        Fatura,
        Teslimat
    }
}