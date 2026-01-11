namespace Sasquatch.Collection.Services;

/// <summary>
/// Service for providing workflow instructions to guide users through data submission steps.
/// Instructions appear as numbered green pills above interactive elements when "Show Instructions" is enabled.
/// </summary>
public interface IInstructionService
{
    /// <summary>
    /// Get the list of instruction steps for a specific workflow and view
    /// </summary>
    /// <param name="workflow">Workflow name (e.g., "Enrollment", "Budget")</param>
    /// <param name="view">View name (e.g., "Index", "Details", "Upload")</param>
    List<InstructionStep> GetInstructions(string workflow, string view);
}

/// <summary>
/// Represents a single instruction step that appears as a pill above an element
/// </summary>
public class InstructionStep
{
    /// <summary>
    /// The data-instruction attribute value of the target element
    /// </summary>
    public string ElementId { get; set; } = string.Empty;

    /// <summary>
    /// The step number (1, 2, 3...) shown in the pill
    /// </summary>
    public int StepNumber { get; set; }

    /// <summary>
    /// The instruction text displayed in the pill
    /// </summary>
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// Implementation of instruction service with workflow-specific step definitions
/// </summary>
public class InstructionService : IInstructionService
{
    /// <summary>
    /// Instruction definitions organized by workflow and view
    /// </summary>
    private static readonly Dictionary<string, Dictionary<string, List<InstructionStep>>> Instructions = new()
    {
        ["Enrollment"] = new()
        {
            ["Index"] = new List<InstructionStep>
            {
                new() { ElementId = "upload-file", StepNumber = 1, Text = "Upload enrollment data from a CSV file OR enter data manually by school" },
                new() { ElementId = "view-submission", StepNumber = 2, Text = "View or edit a submission" }
            },
            ["Details"] = new List<InstructionStep>
            {
                new() { ElementId = "edit-headcount", StepNumber = 1, Text = "Edit headcount or FTE values directly" },
                new() { ElementId = "add-comment", StepNumber = 2, Text = "Add explanation to resolve warnings" },
                new() { ElementId = "submit-approval", StepNumber = 3, Text = "Submit when all errors are resolved" }
            },
            ["Upload"] = new List<InstructionStep>
            {
                new() { ElementId = "file-drop", StepNumber = 1, Text = "Click or drag your enrollment file here" },
                new() { ElementId = "upload-validate", StepNumber = 2, Text = "Click to validate and process" }
            },
            ["ManualEntry"] = new List<InstructionStep>
            {
                new() { ElementId = "school-select", StepNumber = 1, Text = "Select a school to edit" },
                new() { ElementId = "edit-fields", StepNumber = 2, Text = "Enter enrollment counts" },
                new() { ElementId = "save-data", StepNumber = 3, Text = "Save your changes" }
            }
        },
        ["Budget"] = new()
        {
            ["Index"] = new List<InstructionStep>
            {
                new() { ElementId = "upload-budget", StepNumber = 1, Text = "Upload a budget file (CSV or Excel)" },
                new() { ElementId = "view-submission", StepNumber = 2, Text = "View or edit a submission" }
            },
            ["Details"] = new List<InstructionStep>
            {
                new() { ElementId = "edit-amount", StepNumber = 1, Text = "Edit revenue or expenditure amounts" },
                new() { ElementId = "add-comment", StepNumber = 2, Text = "Add explanation for variances" },
                new() { ElementId = "submit-approval", StepNumber = 3, Text = "Submit when budget is balanced" }
            },
            ["Upload"] = new List<InstructionStep>
            {
                new() { ElementId = "budget-type", StepNumber = 1, Text = "Select Original, Revised, or Final" },
                new() { ElementId = "file-drop", StepNumber = 2, Text = "Click or drag your budget file here" },
                new() { ElementId = "upload-validate", StepNumber = 3, Text = "Click to validate and upload" }
            }
        }
    };

    public List<InstructionStep> GetInstructions(string workflow, string view)
    {
        if (Instructions.TryGetValue(workflow, out var workflowInstructions))
        {
            if (workflowInstructions.TryGetValue(view, out var viewInstructions))
            {
                return viewInstructions;
            }
        }

        return new List<InstructionStep>();
    }
}
