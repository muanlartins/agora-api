using Newtonsoft.Json;

public static class DebatesRoute {
  public static void GetRoutes(WebApplication app, WebApplicationBuilder builder) {
    AuthService authService = new AuthService(builder);
    DebatesService debatesService = new DebatesService(builder);

    app.MapPost("/debate", async (HttpRequest request, Debate debate) => {
      Debate createdDebate = await debatesService.CreateDebate(debate);

      return Results.Ok(createdDebate);
    });

    app.MapGet("/debate", async (HttpRequest request, string id) => {
      Debate? debate = await debatesService.GetDebate(id);

      if (debate is null) return Results.BadRequest("Não foi possível encontrar o debate.");

      return Results.Ok(debate);
    });

    app.MapPut("/debate", async (HttpRequest request) => {
      using (StreamReader r = new StreamReader(request.Body)) {
        string bodyString = await r.ReadToEndAsync();

        Debate updatedDebate = JsonConvert.DeserializeObject<Debate>(bodyString)!;

        bool updated = await debatesService.UpdateDebate(updatedDebate);

        if (updated) return Results.Ok("Debate atualizado com sucesso.");
        return Results.BadRequest("Não foi possível atualizar o debate.");
      }
    });

    app.MapDelete("/debate/{id}", async (HttpRequest request, string id) => {
      bool updated = await debatesService.DeleteDebate(id);

      if (updated) return Results.Ok("Debate deletado com sucesso.");
      return Results.BadRequest("Não foi possível deletar o debate.");
    });


    app.MapGet("/debates", async (HttpRequest request) => {
      List<Debate> debates = await debatesService.GetAllDebates();

      return Results.Ok(debates);
    });
  }
}