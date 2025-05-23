﻿using AirQualityMonitoringDashboard.Data;
using AirQualityMonitoringDashboard.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirQualityMonitoringDashboard.Repositories
{
    public class AQIDataRepository : IAQIDataRepository
    {
        private readonly ApplicationDbContext _context;

        public AQIDataRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddReading(AQIData reading)
        {
            _context.AQIData.Add(reading);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AQIData>> GetAllReadingsAsync()
        {
            return await _context.AQIData.ToListAsync();
        }

        public async Task<AQIData> GetReadingByIdAsync(int id)
        {
            return await _context.AQIData.FindAsync(id);
        }

        public async Task<IEnumerable<AQIData>> GetLatestReadingsAsync(int sensorId, int topCount)
        {
            return await _context.AQIData
                .Where(r => r.SensorId == sensorId)
                .OrderByDescending(r => r.RecordedAt)
                .Take(topCount)
                .ToListAsync();
        }

        // Method for historical data filtering
        public async Task<IEnumerable<AQIData>> GetHistoricalReadingsAsync(int sensorId, DateTime startDate, DateTime endDate)
        {
            return await _context.AQIData
                .Where(r => r.SensorId == sensorId && r.RecordedAt >= startDate && r.RecordedAt <= endDate)
                .OrderBy(r => r.RecordedAt) // Order chronologically for charts
                .ToListAsync(); //
        }
    }
}