using ModelLayer.DTOs;  // Fixed: MeasurementRecord is in DTOs
using RepositoryLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RepositoryLayer.Implementations
{
    public sealed class QuantityMeasurementCacheRepository : IQuantityMeasurementRepository
    {
        private static readonly object _lock = new object();
        private static volatile QuantityMeasurementCacheRepository? _instance;
        private List<MeasurementRecord> _cache = new List<MeasurementRecord>();
        private readonly string _filePath = "measurements.json";

        private QuantityMeasurementCacheRepository()
        {
            LoadFromDisk();
        }

        public static QuantityMeasurementCacheRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new QuantityMeasurementCacheRepository();
                        }
                    }
                }
                return _instance;
            }
        }

        public void Save(MeasurementRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            lock (_lock)
            {
                _cache.Add(record);
            }
            SaveToDisk();
        }

        public IEnumerable<MeasurementRecord> GetAll()
        {
            lock (_lock)
            {
                return _cache.ToList();
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _cache.Clear();
            }
            
            if (File.Exists(_filePath))
            {
                try
                {
                    File.Delete(_filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to delete file: {ex.Message}");
                }
            }
        }

        private void SaveToDisk()
        {
            try
            {
                List<MeasurementRecord> cacheCopy;
                lock (_lock)
                {
                    cacheCopy = _cache.ToList();
                }
                
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                };
                
                string jsonString = JsonSerializer.Serialize(cacheCopy, options);
                File.WriteAllText(_filePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to save to disk: {ex.Message}");
            }
        }

        private void LoadFromDisk()
        {
            if (!File.Exists(_filePath))
                return;

            try
            {
                string jsonString = File.ReadAllText(_filePath);
                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                };
                
                var loaded = JsonSerializer.Deserialize<List<MeasurementRecord>>(jsonString, options);
                if (loaded != null)
                {
                    lock (_lock)
                    {
                        _cache = loaded;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to load from disk: {ex.Message}");
                lock (_lock)
                {
                    _cache = new List<MeasurementRecord>();
                }
            }
        }
    }
}