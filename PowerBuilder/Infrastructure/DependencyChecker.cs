using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Text;
using PowerBuilder.Exceptions;
using Serilog;
using PowerBuilder.Utils;

/// <summary>
/// Dependency Checker provides requirement validation for bindings and shared parameter usage
/// </summary>
public class DependencyChecker
{
    private Document _doc;
    public DependencyChecker(Document doc)
    {
        _doc = doc;
    }

    /// <summary>
    /// Validate parameter bindings in the active document.
    /// </summary>
    /// <param name="guid">GUID of shared parameter</param>
    /// <param name="cats">List of built-in categories to target in the binding</param>
    /// <returns></returns>
    public bool ValidateBinding(Guid guid, List<BuiltInCategory> cats)
    {
        Definition targetDef;
        SharedParameterElement spElement = SharedParameterElement.Lookup(_doc, guid);
        
        if (spElement == null) {
            targetDef = ManagedParameterUtils.LookupManagedParameter(_doc, guid) as Definition;
        }
        else {
            targetDef = spElement.GetDefinition();
        }

        CategorySet catSet = CategorySetFromCatList(cats);

        return ValidateDefinitionBinding(targetDef, catSet);
    }

    /// <summary>
    /// Validate parameter bindings in the active document
    /// </summary>
    /// <param name="name">name of the shared parameter</param>
    /// <param name="cats">List of built-in categories to target in the binding</param>
    /// <returns></returns>
    public bool ValidateBinding(string name, List<BuiltInCategory> cats)
    {
        Definition targetDef;
        SharedParameterElement spElement = new FilteredElementCollector(_doc)
            .OfClass(typeof(SharedParameterElement))
            .ToElements()
            .Where(spe => spe.Name == name)
            .Cast<SharedParameterElement>()
            .First();

        if (spElement == null) {
            targetDef = ManagedParameterUtils.LookupManagedParameter(_doc, name) as Definition;
        }
        else {
            targetDef = spElement.GetDefinition();
        }
        CategorySet catSet = CategorySetFromCatList(cats);

        return ValidateDefinitionBinding(targetDef, catSet);
    }

    private bool ValidateDefinitionBinding(Definition def, CategorySet targets)
    {
        BindingMap parameterBinding = _doc.ParameterBindings;
        bool bindingValidated = false;
        CategorySet newBindingTargets = new CategorySet();

        if (parameterBinding.Contains(def))
        {
            InstanceBinding thisBinding = parameterBinding.get_Item(def) as InstanceBinding;
            // need to have a way to specify the expected parameter target (type/instance)
            // parameters may only have one class of targets.  currently this implementation doesn't include
            // and check for type consistency between the parameter and the bound categories

            //TODO: is there a greater need for this set intersection type of Contain(CategorySet self, CategorySet target)
            /*TODO: refine the approach here to appropriately handle expansion of existing bindings that must include
            --i think we have to use type parameters here.  let's get this working for kind of a static instance binding
              because that's all PB is using right now.
            1. identify the current binding type
            2. identify the expected/required binding type
            3. expand the binding if the types match
            4. what to do if the bindings do not match ??
                4.1 disable the service. throw an exception.
            */
            foreach (Category currentCategory in targets){

                if (!thisBinding.Categories.Contains(currentCategory))
                    newBindingTargets.Insert(currentCategory);
            }
        }
        else{

            newBindingTargets = targets;
        }

        if (newBindingTargets.Size > 0){

            StringBuilder catSetString = new StringBuilder();
            foreach (Category target in newBindingTargets) {
                catSetString.AppendLine(target.Name);
            }
            TaskDialog approveNewBinding = new TaskDialog("Approve new Parameter Bindings");
            //TODO: add a way to pass the calling class to this message
            approveNewBinding.MainInstruction = $"Parameter {def.Name} required for some PowerBuilder Feature affecting these Categories"+"\n"+
                $"{catSetString.ToString()}";
            approveNewBinding.MainContent = "Add new Project Parameter bindings?";
            //approveNewBinding.DefaultButton = TaskDialogResult.Ok;
            approveNewBinding.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
            approveNewBinding.DefaultButton = TaskDialogResult.Yes;

            TaskDialogResult res = approveNewBinding.Show();

            if (res == TaskDialogResult.Yes){

                InstanceBinding targetBinding = new InstanceBinding(targets);
                bindingValidated = AddParameterBinding(def, targetBinding);
            }
        }
        else{
            bindingValidated = true;
        }
        if (!bindingValidated){
            throw new MissingBindingException($"Required Parameter {def.Name} not available");
        }

        return bindingValidated;
        
    }

    private CategorySet CategorySetFromCatList(List<BuiltInCategory> catList){

        CategorySet cats = new CategorySet();
        Log.Debug($"Buliding target CategorySet");
        foreach (BuiltInCategory bic in catList){
            
            Category cat = Category.GetCategory(_doc, bic);
            Log.Debug($"\tretrieved cat <{cat.Name},{cat.Id}>");
            cats.Insert(cat);
        }

        return cats;
    }
    private bool AddParameterBinding(Definition def, ElementBinding binding, int mode = 0)
    {
        //reference this
        //https://forums.autodesk.com/t5/revit-api-forum/parameter-bindings/td-p/9235864
        // i think there is yet some nuance to managing this for types vs instance bindings
        // which is controlled at the Definition level.  def can only be for Instance or Type..
        bool result = false;
        try{
            using (Transaction T = new Transaction(_doc, $"add-parameter-binding:{def.Name}")) {
                T.Start();
                _doc.ParameterBindings.Insert(def, binding);
                T.Commit();
            }
            result = true;
        }
        catch (Exception e){
            Log.Error($"Failed Adding Parameter Binding: {e.Message}");
            result = false;
        }

        return result;
    }
}