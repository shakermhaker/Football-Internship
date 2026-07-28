using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Abstract
{
    public interface IBusinessImageService
    {
        Task<Core.Utilities.Results.IResult> AddImageAsync(int businessId, IFormFile file, bool isCover);
        Task<Core.Utilities.Results.IResult> DeleteImageAsync(int imageId);
    }
}
