﻿using System.ComponentModel.DataAnnotations;

namespace Acceloka.Models.DTOS
{
    public class BookedTicketDto
    {
        [Required(ErrorMessage = "Kode tiket wajib diisi")]
        public string TicketCode { get; set; }

        public int Quantity { get; set; }
    }
}
