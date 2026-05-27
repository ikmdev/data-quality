using Newtonsoft.Json;
using PIQI.Components.Models;
using PIQI_Engine.Server.Services;
using static PIQI_Engine.Server.Services.FileCacheService;

namespace PIQI_Engine.Server.Engines
{
    /// <summary>
    /// Provides functionality to load and manage reference data required by the PIQI engine,
    /// including code systems, SAMs, evaluation rubrics, data types, value lists, models, and entities.
    /// </summary>
    public class ReferenceDataEngine
    {
        /// <summary>
        /// Application configuration instance used for accessing configuration settings.
        /// </summary>
        protected readonly IConfiguration _Configuration;

        /// <summary>
        /// Logger instance for recording informational messages, warnings, and errors related to engine operations.
        /// </summary>
        protected readonly ILogger<PIQIEngine> _Logger;

        /// <summary>
        /// File-based caching service used for storing and retrieving reference data to improve performance.
        /// </summary>
        protected readonly FileCacheService _Cache;

        /// <summary>
        /// Synchronization object used to ensure thread-safe access to the cache.
        /// </summary>
        private static object _CacheLock = new object();

        /// <summary>
        /// Initializes a new instance of <see cref="ReferenceDataEngine"/>.
        /// </summary>
        /// <param name="configuration">The application configuration instance.</param>
        /// <param name="logger">The logger for logging engine operations.</param>
        /// <param name="cache">The file cache service for caching reference data.</param>
        public ReferenceDataEngine(IConfiguration configuration, ILogger<PIQIEngine> logger, FileCacheService cache)
        {
            _Configuration = configuration;
            _Logger = logger;
            _Cache = cache;
        }

        #region Main
        /// <summary>
        /// Loads all reference data including code systems, SAMs, evaluation rubrics, data types, value lists, models, and entities.
        /// </summary>
        /// <param name="EvaluationRubricMnemonic">The mnemonic identifier of the evaluation rubric to load.</param>
        /// <param name="evaluation">Optional file containing evaluation rubric JSON data.</param>
        /// <returns>A <see cref="PIQIReferenceData"/> containing the loaded reference data.</returns>
        public PIQIReferenceData LoadRefData(string EvaluationRubricMnemonic, IFormFile? evaluation)
        {
            try
            {
                PIQIReferenceData refData = new PIQIReferenceData();

                // Code systems
                List<CodeSystem>? result1 = LoadCodeSystems();
                if (result1 == null) throw new Exception("Missing or failed to load code systems.");
                refData.CodeSystemList = result1;

                // Sams
                List<SAM>? result2 = LoadSAMs();
                if (result2 == null) throw new Exception("Missing or failed to load SAMs.");
                refData.SAMList = result2;

                // evaluation rubric
                EvaluationRubric? result3 = LoadEvaluationRubric(EvaluationRubricMnemonic, evaluation);
                if (result3 == null) throw new Exception($"Missing or failed to load evaluation rubric: {EvaluationRubricMnemonic}.");
                refData.EvaluationRubric = result3;

                // Data types
                List<DataType>? result4 = LoadDataTypeList();
                if (result4 == null) throw new Exception("Missing or failed to load data type list.");
                refData.DataTypeList = result4;

                // Value list
                List<ValueList>? result5 = LoadValueList();
                if (result5 == null) throw new Exception("Missing or failed to load value list.");
                refData.ValueList = result5;

                // Value list
                List<ValueSet>? result6 = LoadValueSetList();
                if (result6 == null) throw new Exception("Missing or failed to load value set list.");
                refData.ValueSetList = result6;

                // Models
                string? evalModelMnemonic = refData.EvaluationRubric?.Model?.Mnemonic;
                if (evalModelMnemonic == null) throw new Exception("Missing PIQI model mnemonic in evaluation rubric.");

                Model? result8 = LoadModel(evalModelMnemonic);
                if (result8 == null) throw new Exception($"Missing or failed to load model: {evalModelMnemonic}.");
                refData.Model = result8;

                // Entities
                if (refData.Model.DataClasses == null) throw new Exception($"Missing or failed to load entities from the model: {evalModelMnemonic}.");
                refData.EntityModel = new EntityModel(refData.Model);

                return refData;
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Cache Access
        /// <summary>
        /// Retrieves an item of type <typeparamref name="T"/> from the cache using the specified cache key.
        /// </summary>
        /// <typeparam name="T">The type of the cached item.</typeparam>
        /// <param name="cacheBaseKey">The key used to identify the cached item.</param>
        /// <returns>
        /// A <see cref="CacheItem{T}"/> containing the requested item if it exists in the cache; 
        /// otherwise, <c>null</c> or an empty item depending on the cache implementation.
        /// </returns>
        protected CacheItem<T>? GetCacheItem<T>(string cacheBaseKey)
        {
            CacheItem<T>? item;
            _Cache.Get<T>(cacheBaseKey, out item);
            return item;
        }

        /// <summary>
        /// Adds or updates an item of type <typeparamref name="T"/> in the cache under the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the item being cached.</typeparam>
        /// <param name="item">The item to add or update in the cache.</param>
        /// <param name="cacheBaseKey">The key under which the item will be stored in the cache.</param>
        /// <remarks>
        /// This method uses a lock on <see cref="_CacheLock"/> to ensure thread-safe cache updates.
        /// </remarks>
        protected void SetCacheItem<T>(T item, string cacheBaseKey)
        {
            lock (_CacheLock)
            {
                _Cache.AddOrUpdate<T>(cacheBaseKey, item);
            }
        }

        /// <summary>
        /// Removes an item in the cache under the specified key.
        /// </summary>
        /// <param name="cacheBaseKey">The key under which the item will be stored in the cache.</param>
        /// <remarks>
        /// This method uses a lock on <see cref="_CacheLock"/> to ensure thread-safe cache updates.
        /// </remarks>
        protected void RemoveCacheItem(string cacheBaseKey)
        {
            lock (_CacheLock)
            {
                _Cache.Remove(cacheBaseKey);
            }
        }

        #endregion

        #region Code Systems
        /// <summary>
        /// Loads a list of code systems.
        /// </summary>
        /// <returns>A list of <see cref="CodeSystem"/>containing the supported code systems.</returns>
        private List<CodeSystem>? LoadCodeSystems()
        {
            string baseKey = "CODE_SYSTEMS";

            try
            {
                // Get from cache
                CacheItem<List<CodeSystem>>? cacheItem = GetCacheItem<List<CodeSystem>>(baseKey);
                List<CodeSystem>? codeSystems = cacheItem?.Value;

                // Get file path from configuration file
                string? filePath = _Configuration["FilePaths:CodeSystemsPath"];
                if (filePath != null && File.Exists(filePath))
                {
                    DateTime lastModified = File.GetLastWriteTimeUtc(filePath);
                    // Check if the code system is cached/if the cache needs to be updated
                    if (codeSystems == null || cacheItem?.LastModified < lastModified)
                    {
                        // Get failed. Load manually.
                        codeSystems = new List<CodeSystem>();

                        // Read file and deserialize the JSON
                        string json = File.ReadAllText(filePath);

                        codeSystems = JsonConvert.DeserializeObject<CodeSystemRoot>(json)?.CodeSystemLibrary;
                        if (codeSystems == null) throw new Exception("Failed to load code systems");

                        // Put code systems in cache
                        SetCacheItem<List<CodeSystem>>(codeSystems, baseKey);
                    }
                }

                return codeSystems;
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Models

        /// <summary>
        /// Loads a specific model by mnemonic identifier.
        /// </summary>
        /// <param name="modelMnemonic">The mnemonic of the model to load.</param>
        /// <returns>A <see cref="Model"/> containing the requested model.</returns>
        private Model? LoadModel(string modelMnemonic)
        {
            string baseKey = "MODEL";
            string libraryKey = "MODEL_LIBRARY";

            try
            {
                // Get from cache
                CacheItem<List<ReferenceDataProfile>>? libraryCacheItem = GetCacheItem<List<ReferenceDataProfile>>($"{baseKey}|{libraryKey}");
                CacheItem<Model>? modelCacheItem = GetCacheItem<Model>($"{baseKey}|{modelMnemonic}");
                List<ReferenceDataProfile>? library = libraryCacheItem?.Value;
                Model? model = modelCacheItem?.Value;

                // Get file path from configuration file
                string? libraryFilePath = _Configuration["FilePaths:ModelPath"];
                string? modelFilePath = null;
                if (libraryFilePath != null && File.Exists(libraryFilePath))
                {
                    DateTime libraryLastModified = File.GetLastWriteTimeUtc(libraryFilePath);
                    // Create/reset list if needed
                    if (library == null || libraryCacheItem?.LastModified < libraryLastModified)
                        library = new List<ReferenceDataProfile>();

                    // Get the filepath from the library
                    if (library.Any(ep => ep.Mnemonic.Equals(modelMnemonic)))
                        modelFilePath = library.FirstOrDefault(ep => ep.Mnemonic.Equals(modelMnemonic))?.FilePath;
                    else
                    {
                        // Read file and deserialize the JSON to get the list of profiles
                        string profileJson = File.ReadAllText(libraryFilePath);
                        List<ReferenceDataProfile>? modelProfiles = JsonConvert.DeserializeObject<ReferenceDataProfileRoot>(profileJson)?.ModelProfiles;
                        if (modelProfiles == null) throw new Exception("Failed to load model profiles.");

                        // Add updated library to the cache
                        SetCacheItem<List<ReferenceDataProfile>>(modelProfiles, $"{baseKey}|{libraryKey}");

                        // Get the file path for the model
                        modelFilePath = modelProfiles.FirstOrDefault(e => e.Mnemonic.Equals(modelMnemonic))?.FilePath;
                    }

                    if (modelFilePath == null || !File.Exists(modelFilePath)) throw new Exception("File for model is invalid or missing.");

                    // Check if the model has been modified since last cached
                    DateTime modelLastModified = File.GetLastWriteTimeUtc(modelFilePath);
                    if (model == null || modelCacheItem?.LastModified < modelLastModified)
                    {
                        // Deserialize the file into an model
                        string rubricJson = File.ReadAllText(modelFilePath);
                        model = JsonConvert.DeserializeObject<Model>(rubricJson);
                        if (model == null) throw new Exception("Failed to load model.");
                    }
                    // Put the model in the cache
                    SetCacheItem<Model>(model, $"{baseKey}|{modelMnemonic}");
                    return model;
                }
                else
                    throw new Exception("Model library not found.");
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Evaluation rubric

        /// <summary>
        /// Loads evaluation rubric by evaluation rubric mnemonic.
        /// </summary>
        /// <returns>A <see cref="EvaluationRubric"/> with the matching evaluation rubric mnemonic.</returns>
        private EvaluationRubric? LoadEvaluationRubric(string evaluationRubricMnemonic, IFormFile? evaluation)
        {
            string baseKey = "EVALUATION_RUBRIC";
            string libraryKey = "EVALUATION_RUBRIC_LIBRARY";

            try
            {
                if (evaluation != null)
                {
                    if (evaluation.Length == 0)
                        throw new Exception("File not selected");

                    using (var reader = new StreamReader(evaluation.OpenReadStream()))
                    {
                        string rubricJson = reader.ReadToEnd();
                        EvaluationRubric? rubric = JsonConvert.DeserializeObject<EvaluationRubric>(rubricJson);
                        if (rubric == null) throw new Exception("Failed to load evaluation rubric.");
                        return rubric;
                    }
                }
                else
                {
                    // Get from cache
                    CacheItem<List<ReferenceDataProfile>>? libraryCacheItem = GetCacheItem<List<ReferenceDataProfile>>($"{baseKey}|{libraryKey}");
                    CacheItem<EvaluationRubric>? evaluationCacheItem = GetCacheItem<EvaluationRubric>($"{baseKey}|{evaluationRubricMnemonic}");
                    List<ReferenceDataProfile>? library = libraryCacheItem?.Value;
                    EvaluationRubric? evaluationRubric = evaluationCacheItem?.Value;

                    // Get file path from configuration file
                    string? libraryFilePath = _Configuration["FilePaths:EvaluationPath"];
                    string? evaluationFilePath = null;
                    if (libraryFilePath != null && File.Exists(libraryFilePath))
                    {
                        DateTime libraryLastModified = File.GetLastWriteTimeUtc(libraryFilePath);
                        // Create/reset list if needed
                        if (library == null || libraryCacheItem?.LastModified < libraryLastModified)
                            library = new List<ReferenceDataProfile>();

                        // Get the filepath from the library
                        if (library.Any(ep => ep.Mnemonic.Equals(evaluationRubricMnemonic)))
                            evaluationFilePath = library.FirstOrDefault(ep => ep.Mnemonic.Equals(evaluationRubricMnemonic))?.FilePath;
                        else
                        {
                            // Read file and deserialize the JSON to get the list of profiles
                            string profileJson = File.ReadAllText(libraryFilePath);
                            library = JsonConvert.DeserializeObject<ReferenceDataProfileRoot>(profileJson)?.EvaluationProfiles;
                            if (library == null) throw new Exception("Failed to load evaluation profiles.");

                            // Add updated library to the cache
                            SetCacheItem<List<ReferenceDataProfile>>(library, $"{baseKey}|{libraryKey}");

                            // Get the file path for the evaluation rubric
                            evaluationFilePath = library.FirstOrDefault(e => e.Mnemonic.Equals(evaluationRubricMnemonic))?.FilePath;
                        }

                        if (evaluationFilePath != null && File.Exists(evaluationFilePath))
                        {
                            // Check if the evaluation rubric has been modified since last cached
                            DateTime evaluationLastModified = File.GetLastWriteTimeUtc(evaluationFilePath);
                            if (evaluationRubric == null || evaluationCacheItem?.LastModified < evaluationLastModified)
                            {
                                // Deserialize the file into an evaluation rubric
                                string rubricJson = File.ReadAllText(evaluationFilePath);
                                evaluationRubric = JsonConvert.DeserializeObject<EvaluationRubric>(rubricJson);
                                if (evaluationRubric == null) throw new Exception("Failed to load evaluation rubric.");
                            }
                        }
                        else if (evaluationRubric == null) 
                            throw new Exception("File for evaluation rubric is invalid or missing.");

                        // Update name to match profile name
                        if (library?.FirstOrDefault(ep => ep.Mnemonic.Equals(evaluationRubricMnemonic))?.Name != null)
                            evaluationRubric.Name = library?.FirstOrDefault(ep => ep.Mnemonic.Equals(evaluationRubricMnemonic))?.Name;

                        // Put the evaluation rubric in the cache
                        SetCacheItem<EvaluationRubric>(evaluationRubric, $"{baseKey}|{evaluationRubricMnemonic}");
                        return evaluationRubric;
                    }
                    else
                        throw new Exception("Evaluation library not found.");
                }
            }
            catch
            {
                throw;
            }
        }
        
        #endregion

        #region SAMs

        /// <summary>
        /// Loads a list of all supported SAMs.
        /// </summary>
        /// <returns>A list of <see cref="SAM"/> supported.</returns>
        private List<SAM>? LoadSAMs()
        {
            string baseKey = "SAMS";

            try
            {
                // Get from cache
                CacheItem<List<SAM>>? cacheItem = GetCacheItem<List<SAM>>(baseKey);
                List<SAM>? sams = cacheItem?.Value;

                // Get file path from configuration file
                string? filePath = _Configuration["FilePaths:SAMsPath"];
                if (filePath != null && File.Exists(filePath))
                {
                    DateTime lastModified = File.GetLastWriteTimeUtc(filePath);
                    // Check if the code system is cached/if the cache needs to be updated
                    if (sams == null || cacheItem?.LastModified < lastModified)
                    {
                        // Get failed. Load manually.
                        sams = new List<SAM>();

                        // Read file and deserialize the JSON
                        string json = File.ReadAllText(filePath);

                        sams = JsonConvert.DeserializeObject<SAMRoot>(json)?.SAMLibrary;
                        if (sams == null) throw new Exception("Failed to load SAMs");

                        // Put SAMs in cache
                        SetCacheItem<List<SAM>>(sams, baseKey);
                    }
                }

                return sams;
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Data Types

        /// <summary>
        /// Loads a list of all supported data types.
        /// </summary>
        /// <returns>A list of <see cref="DataType"/> supported.</returns>
        private List<DataType>? LoadDataTypeList()
        {
            string baseKey = "DATA_TYPE_LIST";

            try
            {
                // Get from cache
                CacheItem<List<DataType>>? cacheItem = GetCacheItem<List<DataType>>(baseKey);
                List<DataType>? dataTypes = cacheItem?.Value;

                // Get file path from configuration file
                string? filePath = _Configuration["FilePaths:DataTypesPath"];
                if (filePath != null && File.Exists(filePath))
                {
                    DateTime lastModified = File.GetLastWriteTimeUtc(filePath);
                    // Check if the code system is cached/if the cache needs to be updated
                    if (dataTypes == null || cacheItem?.LastModified < lastModified)
                    {
                        // Get failed. Load manually from file
                        dataTypes = new List<DataType>();

                        // Read file and deserialize the JSON
                        string json = File.ReadAllText(filePath);

                        dataTypes = JsonConvert.DeserializeObject<DataTypeRoot>(json)?.DataTypeLibrary;
                        if (dataTypes == null) throw new Exception("Failed to load data types");

                        // Put in cache
                        SetCacheItem<List<DataType>>(dataTypes, baseKey);
                    }
                }

                return dataTypes;
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region ValueList
        /// <summary>
        /// Loads a list of all supported value lists.
        /// </summary>
        /// <returns>A list of <see cref="ValueList"/> supported.</returns>
        private List<ValueList>? LoadValueList()
        {
            string baseKey = "VALUE_LIST";

            try
            {
                // Get from cache
                CacheItem<List<ValueList>>? cacheItem = GetCacheItem<List<ValueList>>(baseKey);
                List<ValueList>? valueList = cacheItem?.Value;

                // Get file path from configuration file
                string? filePath = _Configuration["FilePaths:ValueListPath"];
                if (filePath != null && File.Exists(filePath))
                {
                    DateTime lastModified = File.GetLastWriteTimeUtc(filePath);
                    // Check if the code system is cached/if the cache needs to be updated
                    if (valueList == null || cacheItem?.LastModified < lastModified)
                    {
                        // Get failed. Load manually.
                        valueList = new List<ValueList>();

                        // Read file and deserialize the JSON
                        string json = File.ReadAllText(filePath);

                        valueList = JsonConvert.DeserializeObject<ValueListRoot>(json)?.ValueLibrary;
                        if (valueList == null) throw new Exception("Failed to load value list");

                        // Put value list in cache
                        SetCacheItem<List<ValueList>>(valueList, baseKey);
                    }
                }

                return valueList;
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region ValueSetList
        /// <summary>
        /// Loads a list of all supported value set lists.
        /// </summary>
        /// <returns>A list of<see cref = "ValueSet" /> supported.</returns>
        private List<ValueSet>? LoadValueSetList()
        {
            string baseKey = "VALUE_SET_LIST";

            try
            {
                // Get from cache
                CacheItem<List<ValueSet>>? cacheItem = GetCacheItem<List<ValueSet>>(baseKey);
                List<ValueSet>? valueList = cacheItem?.Value;

                // Get file path from configuration file
                string? filePath = _Configuration["FilePaths:ValueSetListPath"];
                if (filePath != null && File.Exists(filePath))
                {
                    DateTime lastModified = File.GetLastWriteTimeUtc(filePath);
                    // Check if the code system is cached/if the cache needs to be updated
                    if (valueList == null || cacheItem?.LastModified < lastModified)
                    {
                        // Get failed. Load manually.
                        valueList = new List<ValueSet>();

                        // Read file and deserialize the JSON
                        string json = File.ReadAllText(filePath);

                        valueList = JsonConvert.DeserializeObject<ValueSetListRoot>(json)?.ValueSetLibrary;
                        if (valueList == null) throw new Exception("Failed to load value set list");

                        // Put value list in cache
                        SetCacheItem<List<ValueSet>>(valueList, baseKey);
                    }
                }

                return valueList;
            }
            catch
            {
                throw;
            }
        }
        #endregion

    }
}
