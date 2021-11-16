using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net5ConaoleApp.Services
{
    class RandomService
    {
        readonly IConfiguration _config;

        public RandomService(IConfiguration config) 
        {
            _config = config;
        }

        public string GetRandomGuid() 
        {
            return Guid.NewGuid().ToString();        
        }
    }
}
