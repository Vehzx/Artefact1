// anything the player can press interact on
// chests now, doors / NPCs / levers later can use the same interface
public interface IInteractable
{
    void Interact(PlayerInteractor interactor);
}
