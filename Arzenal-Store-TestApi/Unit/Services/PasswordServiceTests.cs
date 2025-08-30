using ArzenalStoreApi.Services.PasswordService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestArzenalStoreApi.Unit.Services
{
    public class PasswordServiceTests
    {
        [Fact]
        public void Hash_ShouldReturnNonEmptyHash()
        {
            var service = new PasswordService();
            var password = "MySecret123";
            var hash = service.Hash(password);

            Assert.False(string.IsNullOrEmpty(hash));
        }

        [Fact]
        public void Verify_ShouldReturnTrueForCorrectPassword_AndFalseForIncorrect()
        {
            var service = new PasswordService();
            var password = "MySecret123";
            var hash = service.Hash(password);

            Assert.True(service.Verify(password, hash));
            Assert.False(service.Verify("WrongPassword", hash));
        }

    }
}
