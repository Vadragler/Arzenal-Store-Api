using ArzenalStoreApi.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Controllers
{
    public class HealthControllerTests
    {
        [Fact]
        public void Get_ReturnsOk_WithHealthyStatus()
        {
            // Arrange
            var controller = new HealthController();

            // Act & Assert
            var result = controller.Get();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            Assert.True(data["Status"] != null && data["Status"].ToString() == "Healthy");
        }
    }
}
