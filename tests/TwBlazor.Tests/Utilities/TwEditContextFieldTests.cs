using Microsoft.AspNetCore.Components.Forms;
using TwBlazor.Utilities;

namespace TwBlazor.Tests.Utilities;

public class TwEditContextFieldTests
{
    private sealed class Model
    {
        public string? Name { get; set; }
    }

    private static FieldIdentifier NameField(Model model) => FieldIdentifier.Create(() => model.Name);

    [Fact]
    public void Unattached_IsNotBound_AndHasNoMessages()
    {
        using var field = new TwEditContextField(() => { });

        Assert.False(field.IsBound);
        Assert.Empty(field.Messages);
        Assert.Null(field.FirstMessage);
        field.NotifyChanged();
    }

    [Fact]
    public void Attach_WithAField_ReadsTheFormsMessages()
    {
        var model = new Model();
        var context = new EditContext(model);
        var store = new ValidationMessageStore(context);
        using var field = new TwEditContextField(() => { });

        field.Attach(context, () => NameField(model));
        store.Add(NameField(model), "Enter a name");

        Assert.True(field.IsBound);
        Assert.Equal("Enter a name", field.FirstMessage);
        Assert.Equal(["Enter a name"], field.Messages);
    }

    [Fact]
    public void Attach_WithoutAField_IsNotBound()
    {
        var context = new EditContext(new Model());
        using var field = new TwEditContextField(() => { });

        field.Attach(context, () => null);

        Assert.False(field.IsBound);
        Assert.Same(context, field.EditContext);
        Assert.Empty(field.Messages);
    }

    [Fact]
    public void Attach_ToTheSameForm_TriesAMissingFieldAgain()
    {
        var model = new Model();
        var context = new EditContext(model);
        using var field = new TwEditContextField(() => { });
        field.Attach(context, () => null);

        field.Attach(context, () => NameField(model));

        Assert.True(field.IsBound);
    }

    [Fact]
    public void NotifyChanged_TellsTheForm()
    {
        var model = new Model();
        var context = new EditContext(model);
        FieldIdentifier? changed = null;
        context.OnFieldChanged += (_, e) => changed = e.FieldIdentifier;
        using var field = new TwEditContextField(() => { });
        field.Attach(context, () => NameField(model));

        field.NotifyChanged();

        Assert.Equal(NameField(model), changed);
        Assert.True(context.IsModified(NameField(model)));
    }

    [Fact]
    public void ValidationStateChange_CallsBack_UntilDetached()
    {
        var context = new EditContext(new Model());
        var calls = 0;
        var field = new TwEditContextField(() => calls++);
        field.Attach(context, () => null);

        context.NotifyValidationStateChanged();
        field.Dispose();
        context.NotifyValidationStateChanged();

        Assert.Equal(1, calls);
        Assert.Null(field.EditContext);
    }

    [Fact]
    public void Attach_ToAnotherForm_MovesItsListener()
    {
        var first = new EditContext(new Model());
        var second = new EditContext(new Model());
        var calls = 0;
        using var field = new TwEditContextField(() => calls++);
        field.Attach(first, () => null);

        field.Attach(second, () => null);
        first.NotifyValidationStateChanged();
        second.NotifyValidationStateChanged();

        Assert.Equal(1, calls);
        Assert.Same(second, field.EditContext);
    }

    [Fact]
    public void Attach_Null_Detaches()
    {
        var context = new EditContext(new Model());
        var calls = 0;
        using var field = new TwEditContextField(() => calls++);
        field.Attach(context, () => null);

        field.Attach(null, () => null);
        context.NotifyValidationStateChanged();

        Assert.Equal(0, calls);
        Assert.Null(field.EditContext);
    }
}
