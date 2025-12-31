using LabelChest.UI.Menus;

namespace LabelChest.UI;

class OptionsManager {
    public List<LComponent> options = new();
    public List<string> optionsTag = new();
    public Dictionary<string, bool> tagVisibility = new();
    private Action<List<LComponent>> onVisibleOptionsChanged;

    public OptionsManager(Action<List<LComponent>> updateAction) {
        onVisibleOptionsChanged = updateAction;
    }

    public void SetUpdateCallback(Action<List<LComponent>> updateAction) {
        onVisibleOptionsChanged = updateAction;
    }

    private void TriggerUpdate() {
        if (onVisibleOptionsChanged != null) {
            onVisibleOptionsChanged(GetVisibleOptions());
        }
    }

    public OptionsManager Add(LComponent option, string tag = "default") {
        options.Add(option);
        optionsTag.Add(tag);
        if (!tagVisibility.ContainsKey(tag)) {
            tagVisibility[tag] = true;
        }
        TriggerUpdate();
        return this;
    }

    public void Display(string tag) {
        if (!tagVisibility.ContainsKey(tag)) return;
        tagVisibility[tag] = true;
        TriggerUpdate();
    }

    public void Display(List<string> tag) {
        bool changed = false;
        tag.ForEach((t) => {
            if (!tagVisibility.ContainsKey(t)) return;
            if (!tagVisibility[t]) {
                tagVisibility[t] = true;
                changed = true;
            }
        });
        if (changed) TriggerUpdate();
    }

    public void Hide(string tag) {
        if (!tagVisibility.ContainsKey(tag)) return;
        tagVisibility[tag] = false;
        TriggerUpdate();
    }

    public void Hide(List<string> tag) {
        bool changed = false;
        tag.ForEach((t) => {
            if (!tagVisibility.ContainsKey(t)) return;
            if (tagVisibility[t]) {
                tagVisibility[t] = false;
                changed = true;
            }
        });
        if (changed) TriggerUpdate();
    }

    public bool IsVisible(string tag) {
        if (!tagVisibility.ContainsKey(tag)) return false;
        return tagVisibility[tag];
    }

    public bool IsVisible(int tagIndex) {
        string tag = optionsTag[tagIndex];
        if (!tagVisibility.ContainsKey(tag)) return false;
        return tagVisibility[tag];
    }

    public List<LComponent> GetVisibleOptions() {
        return options.Where((option, index) => IsVisible(index)).ToList();
    }
}