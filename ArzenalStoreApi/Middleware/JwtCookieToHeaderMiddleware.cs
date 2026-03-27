namespace ArzenalStoreApi.Middleware
{
    public class JwtCookieToHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _cookieName;

        public JwtCookieToHeaderMiddleware(RequestDelegate next, string cookieName = "authToken")
        {
            _next = next;
            _cookieName = cookieName;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Si le header Authorization est vide
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                // Vérifie si le cookie existe
                if (context.Request.Cookies.TryGetValue(_cookieName, out var token))
                {
                    // Injecte le token dans le header Authorization
                    context.Request.Headers.Append("Authorization", $"Bearer {token}");
                }
            }

            // Passe au middleware suivant
            await _next(context);
        }
    }
}