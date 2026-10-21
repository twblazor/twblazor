using System.ComponentModel.DataAnnotations;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Models;

namespace TwBlazor.Tests.Components.Form;

public class TwFormTests : TwBlazorTestBase
{
    private static readonly string[] _plans = ["Free", "Pro"];

    private TwFormTheme theme => Theme.Components.Require<TwFormTheme>();

    private sealed class Person
    {
        [Required(ErrorMessage = "Enter your name")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Enter your email")]
        [EmailAddress(ErrorMessage = "Enter a real email")]
        public string? Email { get; set; }

        [Range(18, 120, ErrorMessage = "You must be 18 or over")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Choose a plan")]
        public string? Plan { get; set; }

        [Required(ErrorMessage = "Choose a room")]
        public string? Room { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "Agree to the terms")]
        public bool Agree { get; set; }

        [Required(ErrorMessage = "Enter a date")]
        public string? Arrival { get; set; }

        [Required(ErrorMessage = "Not shown on any field")]
        public string? Hidden { get; set; }
    }

    private static RenderFragment<EditContext> NameField(Person person) => _ => builder =>
    {
        builder.OpenComponent<TwTextfield<string>>(0);
        builder.AddComponentParameter(1, "Id", "name");
        builder.AddComponentParameter(2, "Label", "Name");
        builder.AddComponentParameter(3, "Value", person.Name);
        builder.AddComponentParameter(4, "ValueChanged", EventCallback.Factory.Create<string>(person, value => person.Name = value));
        builder.AddComponentParameter(5, "ValueExpression", (System.Linq.Expressions.Expression<Func<string>>)(() => person.Name!));
        builder.CloseComponent();
    };

    private IRenderedComponent<TwForm> RenderForm(Person person, RenderFragment<EditContext> content, Action<ComponentParameterCollectionBuilder<TwForm>>? configure = null) =>
        TestContext.Render<TwForm>(p =>
        {
            p.Add(x => x.Id, "form");
            p.Add(x => x.Model, person);
            p.Add(x => x.ChildContent, content);
            configure?.Invoke(p);
        });

    [Fact]
    public void TwForm_RendersAFormWithTheThemeClasses_AndTurnsOffNativeValidation()
    {
        var cut = RenderForm(new Person { Name = "Jo" }, NameField(new Person()), p => p.Add(x => x.Class, "custom-form"));

        var form = cut.Find("form#form");
        Assert.Contains(theme.Base.Split(' ')[0], form.GetAttribute("class"));
        Assert.Contains("custom-form", form.GetAttribute("class"));
        Assert.True(form.HasAttribute("novalidate"));
    }

    [Fact]
    public void TwForm_NativeValidation_LeavesTheBrowsersValidationOn()
    {
        var cut = RenderForm(new Person(), NameField(new Person()), p => p.Add(x => x.NativeValidation, true));

        Assert.False(cut.Find("form").HasAttribute("novalidate"));
    }

    [Fact]
    public void TwForm_WithoutAModelOrEditContext_Throws()
    {
        var error = Assert.Throws<InvalidOperationException>(() => TestContext.Render<TwForm>());
        Assert.Contains("Model", error.Message);
    }

    [Fact]
    public void TwForm_WithBothAModelAndAnEditContext_Throws()
    {
        var person = new Person();
        var error = Assert.Throws<InvalidOperationException>(() => TestContext.Render<TwForm>(p => p
            .Add(x => x.Model, person)
            .Add(x => x.EditContext, new EditContext(person))));
        Assert.Contains("not both", error.Message);
    }

    [Fact]
    public void TwForm_UsesTheEditContextItIsGiven()
    {
        var person = new Person();
        var context = new EditContext(person);
        var cut = TestContext.Render<TwForm>(p => p.Add(x => x.EditContext, context).Add(x => x.ChildContent, NameField(person)));

        Assert.Same(context, cut.Instance.CurrentEditContext);
    }

    [Fact]
    public async Task TwForm_InvalidSubmit_ShowsTheMessageOnTheField_AndRaisesOnInvalidSubmit()
    {
        var person = new Person();
        var invalid = false;
        var valid = false;
        var cut = RenderForm(person, NameField(person), p => p
            .Add(x => x.DataAnnotationsValidation, true)
            .Add(x => x.OnInvalidSubmit, () => invalid = true)
            .Add(x => x.OnValidSubmit, () => valid = true));

        cut.Find("form").Submit();

        await cut.WaitForAssertionAsync(() =>
        {
            var input = cut.Find("#name");
            Assert.Equal("true", input.GetAttribute("aria-invalid"));
            Assert.Equal("name-error", input.GetAttribute("aria-describedby"));
            Assert.Equal("Enter your name", cut.Find("#name-error").TextContent);
        });
        Assert.True(invalid);
        Assert.False(valid);
        Assert.False(cut.Instance.IsValid);
    }

    [Fact]
    public async Task TwForm_FieldChange_RevalidatesTheField()
    {
        var person = new Person();
        var cut = RenderForm(person, NameField(person));
        cut.Find("form").Submit();
        await cut.WaitForElementAsync("#name-error");

        cut.Find("#name").Change("Jo");

        await cut.WaitForAssertionAsync(() =>
        {
            Assert.Empty(cut.FindAll("#name-error"));
            Assert.False(cut.Find("#name").HasAttribute("aria-invalid"));
        });
        Assert.Equal("Jo", person.Name);
    }

    [Fact]
    public async Task TwForm_ValidSubmit_RaisesOnValidSubmit_AndOnSubmit()
    {
        var person = new Person { Name = "Jo", Email = "jo@example.com", Age = 30, Plan = "Pro", Room = "single", Agree = true, Arrival = "01/01/2027", Hidden = "x" };
        var valid = false;
        EditContext? submitted = null;
        var cut = RenderForm(person, NameField(person), p => p
            .Add(x => x.OnValidSubmit, () => valid = true)
            .Add(x => x.OnSubmit, context => submitted = context));

        cut.Find("form").Submit();

        await cut.WaitForAssertionAsync(() => Assert.True(valid));
        Assert.Same(cut.Instance.CurrentEditContext, submitted);
        Assert.True(cut.Instance.IsValid);
        Assert.True(cut.Instance.HasValidated);
    }

    [Fact]
    public async Task TwForm_DataAnnotationsValidationOff_IgnoresTheModelsAttributes()
    {
        var person = new Person();
        var valid = false;
        var cut = RenderForm(person, NameField(person), p => p
            .Add(x => x.DataAnnotationsValidation, false)
            .Add(x => x.OnValidSubmit, () => valid = true));

        cut.Find("form").Submit();

        await cut.WaitForAssertionAsync(() => Assert.True(valid));
        Assert.Empty(cut.FindAll("#name-error"));
    }

    [Fact]
    public async Task TwForm_FieldValidator_StopsTheFormSubmitting()
    {
        var person = new Person { Name = "admin", Email = "a@b.com", Age = 30, Plan = "Pro", Room = "x", Agree = true, Arrival = "x", Hidden = "x" };
        var valid = false;
        RenderFragment<EditContext> content = _ => builder =>
        {
            builder.OpenComponent<TwTextfield<string>>(0);
            builder.AddComponentParameter(1, "Id", "user");
            builder.AddComponentParameter(2, "Value", person.Name);
            builder.AddComponentParameter(3, "Validator", (Func<string, Task<string?>>)(value => Task.FromResult<string?>(value == "admin" ? "That name is taken" : null)));
            builder.CloseComponent();
        };
        var cut = RenderForm(person, content, p => p.Add(x => x.OnValidSubmit, () => valid = true));

        cut.Find("form").Submit();

        await cut.WaitForAssertionAsync(() => Assert.Equal("That name is taken", cut.Find("#user-error").TextContent));
        Assert.False(valid);
    }

    [Fact]
    public async Task TwForm_NumericValidator_StopsTheFormSubmitting()
    {
        var person = new Person { Age = 30 };
        RenderFragment<EditContext> content = _ => builder =>
        {
            builder.OpenComponent<TwNumeric<int>>(0);
            builder.AddComponentParameter(1, "Id", "age");
            builder.AddComponentParameter(2, "Value", person.Age);
            builder.AddComponentParameter(3, "Validator", (Func<int, Task<string?>>)(value => Task.FromResult<string?>(value > 20 ? "Too old" : null)));
            builder.CloseComponent();
        };
        var cut = RenderForm(person, content, p => p.Add(x => x.DataAnnotationsValidation, false).Add(x => x.ShowSummary, true));

        var valid = await cut.InvokeAsync(() => cut.Instance.ValidateAsync());

        Assert.False(valid);
        await cut.WaitForAssertionAsync(() => Assert.Contains("Too old", cut.Find($"#{cut.Instance.Id}-summary").TextContent));
    }

    [Fact]
    public async Task TwForm_InvalidSubmit_WithoutASummary_FocusesTheFirstInvalidField()
    {
        var person = new Person();
        var cut = RenderForm(person, NameField(person));

        cut.Find("form").Submit();

        await cut.WaitForAssertionAsync(() =>
        {
            var focus = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twForm.focusFirstInvalid");
            Assert.Equal("form", focus.Arguments[0]);
        });
    }

    [Fact]
    public async Task TwForm_FocusOnErrorOff_LeavesFocusAlone()
    {
        var person = new Person();
        var cut = RenderForm(person, NameField(person), p => p.Add(x => x.FocusOnError, false));

        cut.Find("form").Submit();
        await cut.WaitForElementAsync("#name-error");

        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier is "twForm.focusFirstInvalid" or "twFocus.focusById");
    }

    [Fact]
    public void TwForm_Summary_IsHiddenUntilTheFormIsSubmitted()
    {
        var person = new Person();
        var cut = RenderForm(person, NameField(person), p => p.Add(x => x.ShowSummary, true));

        Assert.Empty(cut.FindAll("#form-summary"));
    }

    [Fact]
    public async Task TwForm_Summary_ListsEachError_LinkingToItsField_AndTakesFocus()
    {
        var person = new Person();
        var cut = RenderForm(person, NameField(person), p => p.Add(x => x.ShowSummary, true));

        cut.Find("form").Submit();

        var summary = await cut.WaitForElementAsync("#form-summary");
        Assert.Equal("alert", summary.GetAttribute("role"));
        Assert.Equal("-1", summary.GetAttribute("tabindex"));
        Assert.Equal("form-summary-title", summary.GetAttribute("aria-labelledby"));
        Assert.Equal("There is a problem", cut.Find("#form-summary-title").TextContent);
        Assert.Contains(theme.Summary.Split(' ')[0], summary.GetAttribute("class"));

        var link = cut.Find("#form-summary a[href='#name']");
        Assert.Equal("Enter your name", link.TextContent);

        // Errors for properties no field is bound to are listed without a link.
        Assert.Contains(cut.FindAll("#form-summary li"), item => item.TextContent == "Not shown on any field" && item.QuerySelector("a") is null);

        await cut.WaitForAssertionAsync(() =>
        {
            var focus = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twFocus.focusById");
            Assert.Equal("form-summary", focus.Arguments[0]);
        });
    }

    [Fact]
    public async Task TwForm_SummaryLink_MovesFocusToItsField()
    {
        var person = new Person();
        var cut = RenderForm(person, NameField(person), p => p.Add(x => x.ShowSummary, true));
        cut.Find("form").Submit();
        await cut.WaitForElementAsync("#form-summary");

        cut.Find("#form-summary a[href='#name']").Click();

        await cut.WaitForAssertionAsync(() =>
        {
            var focus = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twForm.focusField");
            Assert.Equal("name", focus.Arguments[0]);
        });
    }

    [Fact]
    public async Task TwForm_Summary_UpdatesAsFieldsAreFixed()
    {
        var person = new Person();
        var cut = RenderForm(person, NameField(person), p => p.Add(x => x.ShowSummary, true));
        cut.Find("form").Submit();
        await cut.WaitForElementAsync("#form-summary a[href='#name']");

        cut.Find("#name").Change("Jo");

        await cut.WaitForAssertionAsync(() => Assert.Empty(cut.FindAll("#form-summary a[href='#name']")));
    }

    [Fact]
    public async Task TwForm_ValidateAsync_ChecksTheFormWithoutSubmitting()
    {
        var person = new Person { Name = "Jo" };
        var submitted = false;
        var cut = RenderForm(person, NameField(person), p => p
            .Add(x => x.DataAnnotationsValidation, true)
            .Add(x => x.OnSubmit, () => submitted = true));

        var valid = await cut.InvokeAsync(() => cut.Instance.ValidateAsync());

        Assert.False(valid);
        Assert.False(submitted);
        Assert.True(cut.Instance.HasValidated);
    }

    [Fact]
    public async Task TwForm_IsModified_TracksFieldChanges()
    {
        var person = new Person();
        var cut = RenderForm(person, NameField(person));
        Assert.False(cut.Instance.IsModified);

        cut.Find("#name").Change("Jo");

        await cut.WaitForAssertionAsync(() => Assert.True(cut.Instance.IsModified));
    }

    [Fact]
    public async Task TwForm_FieldInsideAPicker_IsOnlyListedOnce()
    {
        var person = new Person();
        RenderFragment<EditContext> content = _ => builder =>
        {
            builder.OpenComponent<TwDatePicker>(0);
            builder.AddComponentParameter(1, "Id", "arrival");
            builder.AddComponentParameter(2, "Value", person.Arrival);
            builder.AddComponentParameter(3, "ValueChanged", EventCallback.Factory.Create<string>(person, value => person.Arrival = value));
            builder.AddComponentParameter(4, "ValueExpression", (System.Linq.Expressions.Expression<Func<string?>>)(() => person.Arrival));
            builder.CloseComponent();
        };
        var cut = RenderForm(person, content, p => p.Add(x => x.ShowSummary, true));

        cut.Find("form").Submit();

        await cut.WaitForAssertionAsync(() =>
        {
            Assert.Single(cut.FindAll("#form-summary a"), link => link.TextContent == "Enter a date");
            Assert.Equal("true", cut.Find("input#arrival").GetAttribute("aria-invalid"));
            Assert.Equal("Enter a date", cut.Find("#arrival-error").TextContent);
        });
    }

    [Fact]
    public async Task TwForm_RadioGroup_ShowsItsMessage_AndRevalidatesWhenChosen()
    {
        var person = new Person();
        RenderFragment<EditContext> content = _ => builder =>
        {
            builder.OpenComponent<TwRadioGroup<string>>(0);
            builder.AddComponentParameter(1, "Id", "room");
            builder.AddComponentParameter(2, "Legend", "Room");
            builder.AddComponentParameter(3, "Items", new List<RadioGroupItem<string>> { new() { Label = "Single", Value = "single" }, new() { Label = "Double", Value = "double" } });
            builder.AddComponentParameter(4, "Value", person.Room);
            builder.AddComponentParameter(5, "ValueChanged", EventCallback.Factory.Create<string>(person, value => person.Room = value));
            builder.AddComponentParameter(6, "ValueExpression", (System.Linq.Expressions.Expression<Func<string>>)(() => person.Room!));
            builder.CloseComponent();
        };
        var cut = RenderForm(person, content, p => p.Add(x => x.ShowSummary, true));

        cut.Find("form").Submit();
        await cut.WaitForAssertionAsync(() =>
        {
            Assert.Equal("true", cut.Find("fieldset#room").GetAttribute("aria-invalid"));
            Assert.Equal("Choose a room", cut.Find("#room-error").TextContent);
            Assert.NotNull(cut.Find("#form-summary a[href='#room']"));
        });

        cut.FindAll("fieldset#room input[type=radio]")[0].Change(true);

        await cut.WaitForAssertionAsync(() => Assert.Empty(cut.FindAll("#room-error")));
        Assert.Equal("single", person.Room);
    }

    [Fact]
    public async Task TwForm_Checkbox_ShowsItsMessage()
    {
        var person = new Person();
        RenderFragment<EditContext> content = _ => builder =>
        {
            builder.OpenComponent<TwCheckbox<bool>>(0);
            builder.AddComponentParameter(1, "Id", "agree");
            builder.AddComponentParameter(2, "Label", "Agree");
            builder.AddComponentParameter(3, "Value", person.Agree);
            builder.AddComponentParameter(4, "ValueChanged", EventCallback.Factory.Create<bool>(person, value => person.Agree = value));
            builder.AddComponentParameter(5, "ValueExpression", (System.Linq.Expressions.Expression<Func<bool>>)(() => person.Agree));
            builder.CloseComponent();
        };
        var cut = RenderForm(person, content);

        cut.Find("form").Submit();
        await cut.WaitForAssertionAsync(() => Assert.Equal("Agree to the terms", cut.Find("#agree-error").TextContent));

        cut.Find("#agree").Change(true);

        await cut.WaitForAssertionAsync(() => Assert.Empty(cut.FindAll("#agree-error")));
    }

    [Fact]
    public async Task TwForm_Select_ShowsItsMessage()
    {
        var person = new Person();
        RenderFragment<EditContext> content = _ => builder =>
        {
            builder.OpenComponent<TwSelect<string>>(0);
            builder.AddComponentParameter(1, "Id", "plan");
            builder.AddComponentParameter(2, "Label", "Plan");
            builder.AddComponentParameter(3, "Values", _plans);
            builder.AddComponentParameter(4, "SelectedValue", person.Plan);
            builder.AddComponentParameter(5, "SelectedValueChanged", EventCallback.Factory.Create<string>(person, value => person.Plan = value));
            builder.AddComponentParameter(6, "SelectedValueExpression", (System.Linq.Expressions.Expression<Func<string>>)(() => person.Plan!));
            builder.CloseComponent();
        };
        var cut = RenderForm(person, content);

        cut.Find("form").Submit();

        await cut.WaitForAssertionAsync(() => Assert.Equal("Choose a plan", cut.Find("#plan-error").TextContent));
    }

    [Fact]
    public async Task EditForm_TwTextfield_ReportsChanges_AndShowsTheFormsMessage()
    {
        var person = new Person();
        var context = new EditContext(person);
        var changed = new List<string>();
        context.OnFieldChanged += (_, e) => changed.Add(e.FieldIdentifier.FieldName);

        var cut = TestContext.Render<EditForm>(p => p
            .Add(x => x.EditContext, context)
            .Add(x => x.ChildContent, (RenderFragment<EditContext>)(_ => builder =>
            {
                builder.OpenComponent<DataAnnotationsValidator>(0);
                builder.CloseComponent();
                builder.AddContent(1, NameField(person)(context));
            })));

        cut.Find("#name").Change("Jo");
        cut.Find("#name").Change(string.Empty);

        await cut.WaitForAssertionAsync(() => Assert.Equal("Enter your name", cut.Find("#name-error").TextContent));
        Assert.Equal(["Name", "Name"], changed);
    }

    [Fact]
    public async Task TwTextfield_OutsideAForm_IsUnaffected()
    {
        var cut = TestContext.Render<TwTextfield<string>>(p => p.Add(x => x.Id, "solo").Add(x => x.ValueExpression, () => "x"));

        cut.Find("#solo").Change("y");

        Assert.False(cut.Find("#solo").HasAttribute("aria-invalid"));
        Assert.True(await cut.Instance.ValidateAsync());
    }

    [Fact]
    public async Task TwForm_ExplicitInvalidField_StopsTheFormSubmitting_AndIsListed()
    {
        var person = new Person { Name = "Jo", Email = "a@b.com", Age = 30, Plan = "Pro", Room = "x", Agree = true, Arrival = "x", Hidden = "x" };
        var valid = false;
        RenderFragment<EditContext> content = _ => builder =>
        {
            builder.OpenComponent<TwSwitch<bool>>(0);
            builder.AddComponentParameter(1, "Id", "notify");
            builder.AddComponentParameter(2, "Label", "Notify me");
            builder.AddComponentParameter(3, "Invalid", true);
            builder.AddComponentParameter(4, "ErrorMessage", "Switch this on");
            builder.CloseComponent();
        };
        var cut = RenderForm(person, content, p => p.Add(x => x.ShowSummary, true).Add(x => x.OnValidSubmit, () => valid = true));

        cut.Find("form").Submit();

        await cut.WaitForAssertionAsync(() => Assert.Equal("Switch this on", cut.Find("#form-summary a[href='#notify']").TextContent));
        Assert.False(valid);
    }
}
