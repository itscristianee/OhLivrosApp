using Microsoft.AspNetCore.SignalR;

public class LojaHub : Hub
{
    // helper p/ evitar repetir o formato do grupo
    public static string GroupName(int livroId) => $"livro-{livroId}";

    public Task JoinLivro(int livroId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupName(livroId));

    public Task LeaveLivro(int livroId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(livroId));
}
