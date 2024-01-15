using Microsoft.AspNetCore.Mvc;

public static class MembersRoute {
  public static void GetRoutes(WebApplication app, WebApplicationBuilder builder) {
    AuthService authService = new AuthService(builder);
    MembersService membersService = new MembersService(builder);

    app.MapPost("/member", async (HttpRequest request, Member member) => {
      User? user = authService.GetUserByRequest(request);

      if (user is null) return Results.BadRequest("Token de autorização inválido.");

      Member createdMember = await membersService.CreateMember(member);

      return Results.Ok(createdMember);
    });

    app.MapGet("/members", async (HttpRequest request) => {
      User? user = authService.GetUserByRequest(request);

      if (user is null) return Results.BadRequest("Token de autorização inválido.");

      List<Member> members = await membersService.GetAllMembers();

      return Results.Ok(members);
    });
  }
}