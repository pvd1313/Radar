using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.GameSave
{
    public class SaveService : Initializable, ISaveService
    {
        private const string _fileName = "test_data.json";
        private readonly List<ISavedDataReader> _readers;

        private FileDataHandler<SavedData> _dataHandler;
        private SavedData _savedData;

        public SaveService(List<ISavedDataReader> readers)
        {
            _readers = readers;
        }

        public UniTask<InitializationResult> Initialize()
        {
            _dataHandler = new FileDataHandler<SavedData>(Application.persistentDataPath, _fileName);
            return InitializationResult.SuccessNotAsync();
        }

        public void LoadData()
        {
            _savedData = _dataHandler.Load();

            if (_savedData == null)
            {
                BeastLog.Info(this, "No data was found. Initializing data to defaults.");
                NewProgress();
            }
            
            foreach (ISavedDataReader dataReader in _readers)
            {
                dataReader.LoadData(_savedData);
            }
        }

        public void SaveData()
        {
            if (_savedData == null)
            {
                BeastLog.Info(this, "No data was found. Initializing data to defaults.");
                NewProgress();
            }

            foreach (ISavedDataReader savedData in _readers)
            {
                savedData.SaveData(_savedData);
            }
            
            _dataHandler.Save(_savedData);
        }

        private void NewProgress()
        {
            _savedData = new SavedData()
            {
                seed = Random.Range(0,10)
            };
            _dataHandler.Save(_savedData);
        }
    }
}