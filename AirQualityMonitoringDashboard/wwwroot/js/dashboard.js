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
    }).addTo(map); //

    // AQI color and status functions
    function getAQIColor(aqi) {
        if (aqi <= 50) return '#00e400';
        if (aqi <= 100) return '#ffff00';
        if (aqi <= 150) return '#ff7e00';
        if (aqi <= 200) return '#ff0000';
        if (aqi <= 300) return '#8f3f97';
        return '#7e0023';
    } //

    function getAQIStatus(aqi) {
        if (aqi <= 50) return 'Good';
        if (aqi <= 100) return 'Moderate';
        if (aqi <= 150) return 'Unhealthy for Sensitive Groups';
        if (aqi <= 200) return 'Unhealthy';
        if (aqi <= 300) return 'Very Unhealthy';
        return 'Hazardous';
    } //

    const sensorSelect = document.getElementById('sensorSelect');
    const timeButtons = document.querySelectorAll('.time-btn'); // Reference to time period buttons
    let currentSelectedPeriod = 'day'; // Default period
    let currentSelectedSensorId = null; // Track selected sensor
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
                    title: { display: true, text: 'Time' }, // Will be updated dynamically
                    grid: { color: 'rgba(189, 195, 199, 0.3)' }
                },
                y: {
                    beginAtZero: true,
                    max: 400, // Adjust max AQI if needed
                    title: { display: true, text: 'AQI Value' },
                    grid: { color: 'rgba(189, 195, 199, 0.3)' }
                }
            },
            plugins: {
                legend: { display: true, position: 'top' },
                title: {
                    display: true,
                    text: 'Historical AQI Trends', // Can be updated if needed
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
    }); //

    // --- UPDATED: Function to fetch and display historical data ---
    async function updateHistoricalChart(sensorId, period) {
        if (!sensorId || !period) {
            console.log("Sensor ID or period missing for chart update.");
            // Clear the chart if no sensor/period
            aqiChart.data.labels = [];
            aqiChart.data.datasets[0].data = [];
            aqiChart.update();
            return;
        }

        currentSelectedSensorId = sensorId; // Store current sensor
        currentSelectedPeriod = period; // Store current period

        // Highlight active time button
        timeButtons.forEach(btn => {
            btn.classList.toggle('active', btn.dataset.period === period);
        });

        // Calculate start and end dates
        const endDate = new Date();
        let startDate = new Date();
        if (period === 'day') {
            startDate.setDate(endDate.getDate() - 1);
        } else if (period === 'week') {
            startDate.setDate(endDate.getDate() - 7);
        } else if (period === 'month') {
            startDate.setDate(endDate.getDate() - 30);
        }

        // Format dates for API query parameter (ISO string)
        const startDateString = startDate.toISOString();
        const endDateString = endDate.toISOString();

        try {
            // Call the NEW backend endpoint for historical data
            const response = await fetch(`/api/AQIData/historical/${sensorId}?startDate=${startDateString}&endDate=${endDateString}`);

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const historicalData = await response.json();

            if (historicalData && historicalData.length > 0) {
                const labels = historicalData.map(item => {
                    const date = new Date(item.recordedAt);
                    // Adjust date formatting based on period
                    if (period === 'day') {
                        return `${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}`; // HH:MM
                    } else {
                        return date.toLocaleDateString('en-CA'); // YYYY-MM-DD
                    }
                });
                const aqiValues = historicalData.map(item => item.aqi);

                aqiChart.data.labels = labels;
                aqiChart.data.datasets[0].data = aqiValues;
                // Update chart X-axis title
                aqiChart.options.scales.x.title.text = `Time (${period === 'day' ? 'Last 24 Hours' : period === 'week' ? 'Last 7 Days' : 'Last 30 Days'})`;
                aqiChart.update();
            } else {
                console.log("No historical data found for the selected period.");
                aqiChart.data.labels = [];
                aqiChart.data.datasets[0].data = [];
                aqiChart.options.scales.x.title.text = 'Time'; 
                aqiChart.update();
            }
        } catch (error) {
            console.error('Error fetching historical data:', error);
            aqiChart.data.labels = [];
            aqiChart.data.datasets[0].data = [];
            aqiChart.update();
        }
    }

    sensorSelect.addEventListener('change', function () {
        const selectedSensorId = this.value;
        if (selectedSensorId) {
            updateHistoricalChart(selectedSensorId, currentSelectedPeriod);
        } else {
            updateHistoricalChart(null, null);
            currentSelectedSensorId = null; 
        }
    }); 

    timeButtons.forEach(button => {
        button.addEventListener('click', function () {
            const period = this.dataset.period;
            if (currentSelectedSensorId) { 
                updateHistoricalChart(currentSelectedSensorId, period);
            } else {
                currentSelectedPeriod = period;
                timeButtons.forEach(btn => btn.classList.toggle('active', btn === this));
                console.log("Time period selected, waiting for sensor selection.");
            }
        });
    });

    window.showHistorical = function (sensorId) {
        sensorSelect.value = sensorId; 
        updateHistoricalChart(sensorId, currentSelectedPeriod);
        const chartElement = document.getElementById('aqiChart');
        if (chartElement) {
            chartElement.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        }
    };

    // Fetch sensors and update map
    async function updateMap() {
        try {
            const response = await fetch('/Dashboard/GetAllActiveSensors');
            const sensors = await response.json();

            map.eachLayer(layer => {
                if (layer instanceof L.CircleMarker) map.removeLayer(layer);
            });

            sensorSelect.innerHTML = '<option value="">Select a sensor</option>'; 

            sensors.forEach(sensor => {
                if (sensor.status === 'Active') { 
                    const option = document.createElement('option');
                    option.value = sensor.id;
                    option.textContent = `${sensor.name} (${sensor.location})`;
                    sensorSelect.appendChild(option);

                    fetch(`/api/AQIData/latest/${sensor.id}/1`)
                        .then(response => response.ok ? response.json() : Promise.reject(`Failed to fetch latest data for sensor ${sensor.id}`))
                        .then(data => {
                            if (data && data.length > 0) {
                                const latestReading = data[0];
                                const color = getAQIColor(latestReading.aqi);

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
                                        <button onclick="window.showHistorical(${sensor.id})" style="margin-top: 5px; padding: 4px 8px; background: #0288d1; color: white; border: none; border-radius: 3px; cursor: pointer;">View History</button>
                                    </div>
                                `); 
                            } else {
                                console.warn(`No latest AQI data returned for active sensor ${sensor.id}`);
                            }
                        })
                        .catch(error => console.error(`Error fetching latest AQI for sensor ${sensor.id}:`, error));
                }
            });
        } catch (error) {
            console.error('Error fetching sensor data:', error);
        }
    }

    // Initial updates and periodic refresh
    updateMap(); 
    setInterval(updateMap, 300000); // Refresh map every 5 minutes

    document.querySelector(`.time-btn[data-period='${currentSelectedPeriod}']`)?.classList.add('active');

    // Dark mode toggle
    const modeSwitch = document.getElementById('modeSwitch');
    if (modeSwitch) {
        const modeLabel = document.querySelector('.mode-label');
        modeSwitch.addEventListener('change', function () {
            if (this.checked) {
                document.body.classList.add('dark-mode');
                if (modeLabel) modeLabel.textContent = 'Dark';
            } else {
                document.body.classList.remove('dark-mode');
                if (modeLabel) modeLabel.textContent = 'Light';
            }
        });
    }
});