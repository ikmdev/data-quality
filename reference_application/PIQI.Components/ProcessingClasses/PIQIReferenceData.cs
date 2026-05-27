namespace PIQI.Components.Models
{
    /// <summary>
    /// Holds reference data for the PIQI evaluation engine.
    /// </summary>
    public class PIQIReferenceData
    {
        #region Properties
        /// <summary>
        /// The main model used for PIQI evaluation.
        /// </summary>
        public Model Model { get; set; }

        /// <summary>
        /// Optional base model used for inheritance.
        /// </summary>
        public Model? BaseModel { get; set; }

        /// <summary>
        /// List of available code systems.
        /// </summary>
        public List<CodeSystem> CodeSystemList { get; set; }

        /// <summary>
        /// List of available SAMs.
        /// </summary>
        public List<SAM> SAMList { get; set; }

        /// <summary>
        /// The entity model used for PIQI evaluation.
        /// </summary>
        public EntityModel EntityModel { get; set; }

        /// <summary>
        /// The evaluation rubric used for scoring.
        /// </summary>
        public EvaluationRubric EvaluationRubric { get; set; }

        /// <summary>
        /// List of evaluation criteria.
        /// </summary>
        public List<EvaluationCriterion> CriteriaList { get; set; }

        /// <summary>
        /// List of data types used by entities.
        /// </summary>
        public List<DataType> DataTypeList { get; set; }

        /// <summary>
        /// List of value lists used for codeable data.
        /// </summary>
        public List<ValueList> ValueList { get; set; }

        /// <summary>
        /// Gets or sets the collection of individual value sets available.
        /// </summary>
        public List<ValueSet> ValueSetList { get; set; }

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of <see cref="PIQIReferenceData"/> with empty collections.
        /// </summary>
        public PIQIReferenceData()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PIQIReferenceData"/> with a specific entity model.
        /// </summary>
        /// <param name="entityModel">The entity model to use.</param>
        public PIQIReferenceData(EntityModel entityModel)
        {
            Initialize();
            EntityModel = entityModel;
        }

        /// <summary>
        /// Initializes all lists to empty collections.
        /// </summary>
        private void Initialize()
        {
            CodeSystemList = new List<CodeSystem>();
            SAMList = new List<SAM>();
            CriteriaList = new List<EvaluationCriterion>();
            DataTypeList = new List<DataType>();
            ValueList = new List<ValueList>();
            ValueSetList = new List<ValueSet>();
        }
        #endregion

        #region Get Methods
        /// <summary>
        /// Gets a SAM by its mnemonic.
        /// </summary>
        /// <param name="mnemonic">The mnemonic of the SAM.</param>
        /// <returns>The <see cref="SAM"/> if found; otherwise, null.</returns>
        public SAM? GetSAM(string mnemonic)
        {
            return SAMList.FirstOrDefault(s => s.Mnemonic?.Equals(mnemonic) == true);
        }

        /// <summary>
        /// Gets an evaluation criterion for a specific entity and SAM.
        /// </summary>
        /// <param name="evaluationRubric">The evaluation rubric to search.</param>
        /// <param name="entityMnemonic">The entity mnemonic.</param>
        /// <param name="samMnemonic">The SAM mnemonic.</param>
        /// <param name="criterionSequence">The sequence number of the criteria.</param>
        /// <param name="isConditional">Whether to search conditional criteria.</param>
        /// <returns>The matching <see cref="EvaluationCriterion"/> or null.</returns>
        public EvaluationCriterion? GetEvaluationCriterion(EvaluationRubric evaluationRubric, string entityMnemonic, string samMnemonic, int criterionSequence, bool isConditional = false)
        {
            if (evaluationRubric?.Criteria == null) return null;

            return isConditional
                ? evaluationRubric.Criteria.FirstOrDefault(c => c.ConditionalSAM == samMnemonic && c.Entity == entityMnemonic)
                : evaluationRubric.Criteria.FirstOrDefault(c => c.SAMMnemonic == samMnemonic && c.Entity == entityMnemonic && c.Sequence == criterionSequence);
        }

        /// <summary>
        /// Gets an entity by its mnemonic.
        /// </summary>
        /// <param name="mnemonic">The entity mnemonic.</param>
        /// <returns>The <see cref="Entity"/> if found; otherwise, null.</returns>
        public Entity? GetEntity(string mnemonic)
        {
            return EntityModel?.EntityList.FirstOrDefault(e => e.Mnemonic?.Equals(mnemonic) == true);
        }

        /// <summary>
        /// Gets an entity Class by attribute entity mnemonic.
        /// </summary>
        /// <param name="mnemonic">The attribute entity mnemonic.</param>
        /// <returns>The <see cref="Entity"/> if found; otherwise, null.</returns>
        public Entity? GetEntityClass(string mnemonic)
        {
            if (EntityModel.Root.Children == null) return null;
            var dataClass = EntityModel.Root.Children?.FirstOrDefault(c => c.Mnemonic.Equals(mnemonic));
            if (dataClass != null) return dataClass;
            foreach (var classEntity in EntityModel.Root.Children)
            {
                var elementEntity = classEntity.Children?.FirstOrDefault();
                var attributeEntity = elementEntity?.Children?.FirstOrDefault(e => e.Mnemonic?.Equals(mnemonic) == true);
                if (attributeEntity != null) return classEntity;
            }
            return null;
        }

        /// <summary>
        /// Gets a code system by its identifier (name, mnemonic, FHIR URI, or other identifiers).
        /// </summary>
        /// <param name="codeSystemIdentifier">The code system identifier.</param>
        /// <returns>The <see cref="CodeSystem"/> if found; otherwise, null.</returns>
        public CodeSystem GetCodeSystem(string codeSystemIdentifier)
        {
            return CodeSystemList.FirstOrDefault(cs =>
                cs.Name?.Equals(codeSystemIdentifier) == true ||
                cs.Mnemonic?.Equals(codeSystemIdentifier) == true ||
                cs.FhirUri?.Equals(codeSystemIdentifier) == true ||
                cs.CodeSystemIdentifiers?.Any(csi => csi?.Equals(codeSystemIdentifier) == true) == true
            );
        }

        /// <summary>
        /// Gets a value list by its mnemonic.
        /// </summary>
        /// <param name="mnemonic">The value list mnemonic.</param>
        /// <returns>The <see cref="ValueList"/> if found; otherwise, null.</returns>
        public ValueList GetValueList(string mnemonic)
        {
            return ValueList.FirstOrDefault(v => v.Mnemonic?.Equals(mnemonic) == true);
        }

        /// <summary>
        /// Gets a value set by its mnemonic.
        /// </summary>
        /// <param name="mnemonic">The value set mnemonic.</param>
        /// <returns>The <see cref="ValueSet"/> if found; otherwise, null.</returns>
        public ValueSet GetValueSet(string mnemonic)
        {
            return ValueSetList.FirstOrDefault(v => v.Mnemonic?.Equals(mnemonic) == true);
        }
        #endregion
    }
}
