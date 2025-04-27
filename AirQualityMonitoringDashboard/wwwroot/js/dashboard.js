document.addEventListener('DOMContentLoaded', function () {
    console.log('Dashboard initializing...');

    // Initialize global aqiSensorsData for alert system
    window.aqiSensorsData = [];

    // Set current date
    document.getElementById('currentDate').textContent = new Date().toLocaleString('en-US', {
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });

    // Initialize Leaflet Map
    var map = L.map('map').setView([6.9271, 79.8612], 12); // Colombo coordinates
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    // AQI color and status functions
    function getAQIColor(aqi) {
        if (aqi <= 50) return '#00e400';
        if (aqi <= 100) return '#ffff00';
        if (aqi <= 150) return '#ff7e00';
        if (aqi <= 200) return '#ff0000';
        if (aqi <= 300) return '#8f3f97';
        return '#7e0023';
    }

    function getAQIStatus(aqi) {
        if (aqi <= 50) return 'Good';
        if (aqi <= 100) return 'Moderate';
        if (aqi <= 150) return 'Unhealthy for Sensitive Groups';
        if (aqi <= 200) return 'Unhealthy';
        if (aqi <= 300) return 'Very Unhealthy';
        return 'Hazardous';
    }

    // Fetch sensors and update map
    async function updateMap() {
        try {
            const response = await fetch('/Dashboard/GetAllActiveSensors');
            const sensors = await response.json();

            map.eachLayer(layer => {
                if (layer instanceof L.CircleMarker) map.removeLayer(layer);
            });

            // Update sensor select dropdown
            const sensorSelect = document.getElementById('sensorSelect');
            sensorSelect.innerHTML = '<option value="">Select a sensor</option>';

            // Clear existing sensor data in global array
            window.aqiSensorsData = [];

            sensors.forEach(sensor => {
                if (sensor.status === 'Active') {
                    // Add to sensor dropdown
                    const option = document.createElement('option');
                    option.value = sensor.id;
                    option.textContent = `${sensor.name} (${sensor.location})`;
                    sensorSelect.appendChild(option);

                    // Add to global sensors data for alert system
                    window.aqiSensorsData.push({
                        id: sensor.id,
                        name: sensor.name,
                        location: sensor.location,
                        lat: sensor.latitude,
                        lng: sensor.longitude,
                        aqi: 0, // Will be updated when we get AQI data
                        active: sensor.status === 'Active'
                    });

                    fetch(`/Dashboard/GetLatestAQIData?sensorId=${sensor.id}&count=1`)
                        .then(response => response.json())
                        .then(data => {
                            if (data && data.length > 0) {
                                const latestReading = data[0];
                                const color = getAQIColor(latestReading.aqi);

                                // Update AQI in global sensors data for alerts
                                const sensorData = window.aqiSensorsData.find(s => s.id === sensor.id);
                                if (sensorData) {
                                    sensorData.aqi = latestReading.aqi;

                                    // Call alert system if available
                                    if (window.aqiAlertSystem && typeof window.aqiAlertSystem.checkAlerts === 'function') {
                                        window.aqiAlertSystem.checkAlerts(window.aqiSensorsData);
                                    }
                                }

                                const marker = L.circleMarker([sensor.latitude, sensor.longitude], {
                                    radius: 12,
                                    fillColor: color,
                                    color: '#ffffff',
                                    weight: 2,
                                    opacity: 1,
                                    fillOpacity: 0.8,
                                }).addTo(map);

                                marker.bindPopup(`
                                    <div style="background: #ffffff; padding: 8px; border-radius: 4px; color: #333;">
                                        <b style="color: #0288d1;">${sensor.name}</b><br>
                                        Location: ${sensor.location}<br>
                                        AQI: <span style="color: ${color}; font-weight: bold;">${latestReading.aqi}</span><br>
                                        Status: ${getAQIStatus(latestReading.aqi)}<br>
                                        <button onclick="showHistorical(${sensor.id})" style="margin-top: 5px; padding: 4px 8px; background: #0288d1; color: white; border: none; border-radius: 3px; cursor: pointer;">View History</button>
                                    </div>
                                `);
                            }
                        })
                        .catch(error => {
                            console.error(`Error fetching AQI data for sensor ${sensor.id}:`, error);
                        });
                }
            });
        } catch (error) {
            console.error('Error fetching sensor data:', error);

            // If using simulated data as fallback
            simulateSensorData();
        }
    }

    // Fallback simulation for when API fails
    function simulateSensorData() {
        console.log('Using simulated sensor data as fallback');

        // Simulated sensor data
        let simulatedSensors = [
            { id: 1, lat: 6.9271, lng: 79.8612, name: 'Colombo Fort', location: 'Fort', aqi: 0, active: true },
            { id: 2, lat: 6.9350, lng: 79.8487, name: 'Maradana', location: 'Maradana', aqi: 0, active: true },
            { id: 3, lat: 6.9147, lng: 79.8776, name: 'Borella', location: 'Borella', aqi: 0, active: true },
            { id: 4, lat: 6.8905, lng: 79.8685, name: 'Dehiwala', location: 'Dehiwala', aqi: 0, active: true },
            { id: 5, lat: 6.9650, lng: 79.8750, name: 'Wellawatte', location: 'Wellawatte', aqi: 0, active: true }
        ];

        // Clear map markers
        map.eachLayer(layer => {
            if (layer instanceof L.CircleMarker) map.removeLayer(layer);
        });

        // Update sensor select dropdown
        const sensorSelect = document.getElementById('sensorSelect');
        sensorSelect.innerHTML = '<option value="">Select a sensor</option>';

        // Update global sensor data
        window.aqiSensorsData = simulatedSensors;

        // Process each simulated sensor
        simulatedSensors.forEach(sensor => {
            if (sensor.active) {
                // Add to sensor dropdown
                const option = document.createElement('option');
                option.value = sensor.id;
                option.textContent = `${sensor.name} (${sensor.location})`;
                sensorSelect.appendChild(option);

                // Generate random AQI
                sensor.aqi = Math.floor(Math.random() * 300);
                const color = getAQIColor(sensor.aqi);

                const marker = L.circleMarker([sensor.lat, sensor.lng], {
                    radius: 12,
                    fillColor: color,
                    color: '#ffffff',
                    weight: 2,
                    opacity: 1,
                    fillOpacity: 0.8,
                }).addTo(map);

                marker.bindPopup(`
                    <div style="background: #ffffff; padding: 8px; border-radius: 4px; color: #333;">
                        <b style="color: #0288d1;">${sensor.name}</b><br>
                        Location: ${sensor.location}<br>
                        AQI: <span style="color: ${color}; font-weight: bold;">${sensor.aqi}</span><br>
                        Status: ${getAQIStatus(sensor.aqi)}<br>
                        <button onclick="showHistorical(${sensor.id})" style="margin-top: 5px; padding: 4px 8px; background: #0288d1; color: white; border: none; border-radius: 3px; cursor: pointer;">View History</button>
                    </div>
                `);
            }
        });

        // Call alert system for simulated data
        if (window.aqiAlertSystem && typeof window.aqiAlertSystem.checkAlerts === 'function') {
            window.aqiAlertSystem.checkAlerts(window.aqiSensorsData);
        }
    }

    // Chart setup for historical AQI data
    const ctx = document.getElementById('aqiChart').getContext('2d');
    const aqiChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: [],
            datasets: [{
                label: 'Air Quality Index (AQI)',
                data: [],
                borderColor: '#0288d1',
                backgroundColor: 'rgba(2, 136, 209, 0.1)',
                borderWidth: 2.5,
                pointBackgroundColor: '#0288d1',
                pointBorderColor: '#ffffff',
                pointBorderWidth: 1.5,
                pointRadius: 4,
                pointHoverRadius: 6,
                fill: true,
                tension: 0.3
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: {
                    title: { display: true, text: 'Time (HH:MM)' },
                    grid: { color: 'rgba(189, 195, 199, 0.3)' }
                },
                y: {
                    beginAtZero: true,
                    max: 400,
                    title: { display: true, text: 'AQI Value' },
                    grid: { color: 'rgba(189, 195, 199, 0.3)' }
                }
            },
            plugins: {
                legend: { display: true, position: 'top' },
                title: {
                    display: true,
                    text: 'Historical AQI Trends',
                    color: '#0288d1',
                    font: { size: 18, weight: 'bold' }
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return `AQI: ${context.parsed.y}`;
                        }
                    }
                }
            }
        }
    });

    async function updateHistoricalChart(sensorId) {
        try {
            const response = await fetch(`/Dashboard/GetLatestAQIData?sensorId=${sensorId}&count=24`);
            const historicalData = await response.json();

            if (historicalData && historicalData.length > 0) {
                const labels = historicalData.map(item => {
                    const date = new Date(item.recordedAt);
                    return `${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`;
                }).reverse();

                const aqiValues = historicalData.map(item => item.aqi).reverse();

                aqiChart.data.labels = labels;
                aqiChart.data.datasets[0].data = aqiValues;
                aqiChart.update();
            }
        } catch (error) {
            console.error('Error fetching historical data:', error);

            // Fallback to simulated historical data
            simulateHistoricalData(sensorId);
        }
    }

    // Fallback simulation for historical data
    function simulateHistoricalData(sensorId) {
        console.log('Using simulated historical data for sensor:', sensorId);

        // Generate simulated time labels (last 24 hours)
        const labels = [];
        const now = new Date();
        for (let i = 23; i >= 0; i--) {
            const time = new Date(now.getTime() - i * 60 * 60 * 1000);
            labels.push(`${time.getHours().toString().padStart(2, '0')}:${time.getMinutes().toString().padStart(2, '0')}`);
        }

        // Generate simulated AQI values
        const aqiValues = [];
        const sensor = window.aqiSensorsData.find(s => s.id == sensorId);
        if (sensor) {
            let baseAqi = sensor.aqi || Math.floor(Math.random() * 150) + 50; // Start near current AQI or random moderate value

            for (let i = 0; i < 24; i++) {
                // Add some random variation to simulate realistic changes
                const variation = Math.floor(Math.random() * 40) - 20; // -20 to +20
                const aqi = Math.max(0, Math.min(500, baseAqi + variation)); // Clamp to valid AQI range
                aqiValues.push(aqi);

                // Adjust base AQI slightly for next hour (trending up or down slightly)
                baseAqi += Math.floor(Math.random() * 10) - 5; // -5 to +5
            }
        } else {
            // No sensor found, generate completely random data
            for (let i = 0; i < 24; i++) {
                aqiValues.push(Math.floor(Math.random() * 300));
            }
        }

        // Update chart
        aqiChart.data.labels = labels;
        aqiChart.data.datasets[0].data = aqiValues;
        aqiChart.update();
    }

    const sensorSelect = document.getElementById('sensorSelect');
    sensorSelect.addEventListener('change', function () {
        const selectedSensorId = this.value;
        if (selectedSensorId) {
            updateHistoricalChart(selectedSensorId);
        }
    });

    window.showHistorical = function (sensorId) {
        sensorSelect.value = sensorId;
        updateHistoricalChart(sensorId);
    };

    // Dark mode toggle
    const modeSwitch = document.getElementById('modeSwitch');
    const modeLabel = document.querySelector('.mode-label');
    modeSwitch.addEventListener('change', function () {
        if (this.checked) {
            document.body.classList.add('dark-mode');
            modeLabel.textContent = 'Dark';
        } else {
            document.body.classList.remove('dark-mode');
            modeLabel.textContent = 'Light';
        }
    });

    // Initial updates and periodic refresh
    updateMap();
    setInterval(updateMap, 300000); // Refresh every 5 minutes

    console.log('Dashboard initialization complete');
});