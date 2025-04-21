document.addEventListener('DOMContentLoaded', function () {
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
            
            sensors.forEach(sensor => {
                if (sensor.status === 'Active') {
                    const option = document.createElement('option');
                    option.value = sensor.id;
                    option.textContent = `${sensor.name} (${sensor.location})`;
                    sensorSelect.appendChild(option);

                    fetch(`/Dashboard/GetLatestAQIData?sensorId=${sensor.id}&count=1`)
                        .then(response => response.json())
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
                                        <button onclick="showHistorical(${sensor.id})" style="margin-top: 5px; padding: 4px 8px; background: #0288d1; color: white; border: none; border-radius: 3px; cursor: pointer;">View History</button>
                                    </div>
                                `);
                            }
                        });
                }
            });
        } catch (error) {
            console.error('Error fetching sensor data:', error);
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
        }
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
});