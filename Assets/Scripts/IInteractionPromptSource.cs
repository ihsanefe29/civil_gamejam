public interface IInteractionPromptSource
{
    // Returns false when this object's action is not currently possible.
    bool TryGetInteractionPrompt(out string prompt);
}
