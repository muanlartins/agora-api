using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

public static class MembersRoute {
  public static void GetRoutes(WebApplication app, WebApplicationBuilder builder) {
    AuthService authService = new AuthService(builder);
    MembersService membersService = new MembersService(builder);

    app.MapPost("/member", async (HttpRequest request, Member member) => {
      Member createdMember = await membersService.CreateMember(member);

      return Results.Ok(createdMember);
    });

    app.MapGet("/members", async (HttpRequest request) => {
      List<Member> members = await membersService.GetAllMembers();

      return Results.Ok(members);
    });

    app.MapGet("/member", async (HttpRequest request, string id) => {
      Member? member = await membersService.GetMember(id);

      if (member is null) return Results.BadRequest("Não foi possível encontrar o membro.");

      return Results.Ok(member);
    });

    app.MapPut("/member", async (HttpRequest request) => {
      using (StreamReader r = new StreamReader(request.Body)) {
        string bodyString = await r.ReadToEndAsync();

        Member updatedMember = JsonConvert.DeserializeObject<Member>(bodyString)!;

        bool updated = await membersService.UpdateMember(updatedMember);

        if (updated) return Results.Ok("Membro atualizado com sucesso.");
        return Results.BadRequest("Não foi possível atualizar o membro.");
      }
    });

    app.MapPost("/member/pfp", async (HttpContext context) => {
      IFormFile file = context.Request.Form.Files[0];

      using (MemoryStream stream = new MemoryStream()) {
        await file.CopyToAsync(stream);

        await membersService.UploadMemberPfp(file.FileName, stream);
      }

      return Results.Ok();
    });

    app.MapDelete("/member/{id}", async (HttpRequest request, string id) => {
      bool updated = await membersService.DeleteMember(id);

      if (updated) return Results.Ok("Membro deletado com sucesso.");
      return Results.BadRequest("Não foi possível deletar o membro.");
    });
  }
}