export const environment = {
  production: true,

  // Auth-Service (ASP.NET API)
  apiBaseUrl: 'http://localhost:5000/api',

  // Teams-Service (Spring Boot)
  teamsApiUrl: 'http://teams-service:8082/api/teams',
  
  // Players-Service (Node/Laravel o Express)
  playersApiUrl: 'http://players-service:3000/api/players',
    // --- OAuth Redirect (coincide con appsettings.json) ---
  oauthRedirect: 'http://localhost/oauth/callback'
};
