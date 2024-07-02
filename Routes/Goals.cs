using Newtonsoft.Json;

public static class GoalsRoute {
  public static void GetRoutes(WebApplication app, WebApplicationBuilder builder) {
    AuthService authService = new AuthService(builder);
    GoalsService goalsService = new GoalsService(builder);

    app.MapPost("/goal", async (HttpRequest request, Goal goal) => {
      Goal createdGoal = await goalsService.CreateGoal(goal);

      return Results.Ok(createdGoal);
    });

    app.MapGet("/goals", async (HttpRequest request) => {
      List<Goal> goals = await goalsService.GetAllGoals();

      return Results.Ok(goals);
    });

    app.MapGet("/goal", async (HttpRequest request, string id) => {
      Goal? goal = await goalsService.GetGoal(id);

      if (goal is null) return Results.BadRequest("Não foi possível encontrar a meta.");

      return Results.Ok(goal);
    });

    app.MapPut("/goal", async (HttpRequest request) => {
      using (StreamReader r = new StreamReader(request.Body)) {
        string bodyString = await r.ReadToEndAsync();

        Goal updatedGoal = JsonConvert.DeserializeObject<Goal>(bodyString)!;

        bool updated = await goalsService.UpdateGoal(updatedGoal);

        if (updated) return Results.Ok("Meta atualizada com sucesso.");
        return Results.BadRequest("Não foi possível atualizar a meta.");
      }
    });

    app.MapDelete("/goal/{id}", async (HttpRequest request, string id) => {
      bool updated = await goalsService.DeleteGoal(id);

      if (updated) return Results.Ok("Meta deletada com sucesso.");
      return Results.BadRequest("Não foi possível deletar a meta.");
    });
  }
}