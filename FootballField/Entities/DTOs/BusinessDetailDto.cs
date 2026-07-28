using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.DTOs
{
    public class BusinessDetailDto
    {
        public int BusinessId { get; set; }
        public string Name { get; set; }
        public string City { get; set; }     // District tablosundan veya direkt City tablosundan çekiyorsan
        public string District { get; set; }
        public string FullAddress { get; set; }

        // İşletmenin kapak resmi ve diğer resimlerinin yollarını tutacak liste
        public List<BusinessImageDto> Images { get; set; }
    }
}
