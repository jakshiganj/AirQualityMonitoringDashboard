document.addEventListener('DOMContentLoaded', function () {
    // Set current date
    document.getElementById('currentDate').textContent = new Date().toLocaleString({
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });

    // Initialize Leaflet Map
    var map = L.map('map').setView([6.9271, 79.8612], 12); // Colombo coordinates
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png',
        {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        }).addTo(map);



    // Simulated sensor data (to be replaced by backend API fetch)
    let sensors = [
        { id: 1, lat: 6.9271, lng: 79.8612, name: 'Colombo Fort', aqi: 0, active: true },
        { id: 2, lat: 6.9350, lng: 79.8487, name: 'Maradana', aqi: 0, active: true },
        { id: 3, lat: 6.9147, lng: 79.8776, name: 'Borella', aqi: 0, active: true },
        { id: 4, lat: 6.8905, lng: 79.8685, name: 'Dehiwala', aqi: 0, active: true },
        { id: 5, lat: 6.9650, lng: 79.8750, name: 'Wellawatte', aqi: 0, active: true },
        { id: 6, lat: 6.8860, lng: 79.9180, name: 'Mount Lavinia', aqi: 0, active: true },
        { id: 7, lat: 6.9340, lng: 79.8830, name: 'Cinnamon Gardens', aqi: 0, active: true },
        { id: 8, lat: 6.9110, lng: 79.8490, name: 'Pettah', aqi: 0, active: true },
        { id: 9, lat: 6.9540, lng: 79.9120, name: 'Rajagiriya', aqi: 0, active: true },
        { id: 10, lat: 6.8730, lng: 79.8850, name: 'Nugegoda', aqi: 0, active: true },
        { id: 11, lat: 6.8420, lng: 80.0020, name: 'Homagama', aqi: 0, active: true },
        { id: 12, lat: 6.8450, lng: 80.0100, name: 'Godagama', aqi: 0, active: true }
    ];

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

    // Update map with sensor data
    function updateMap() {
        map.eachLayer(layer => {
            if (layer instanceof L.CircleMarker) map.removeLayer(layer);
        });

        sensors.forEach(sensor => {
            if (sensor.active) {
                sensor.aqi = Math.floor(Math.random() * 300); // Simulated AQI (replace with API data)
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
                        AQI: <span style="color: ${color}; font-weight: bold;">${sensor.aqi}</span><br>
                        Status: ${getAQIStatus(sensor.aqi)}<br>
                        <button onclick="showHistorical(${sensor.id})">View History</button>
                    </div>
                `);
            }
        });

        // Update system dashboard (admin only)
        // if (document.getElementById('activeSensors')) {
        //     document.getElementById('activeSensors').textContent = sensors.filter(s => s.active).length;
        // }
    }


    // Chart setup for historical AQI data
    const ctx = document.getElementById('aqiChart').getContext('2d');
    const aqiChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: [], // Time labels will be populated dynamically
            datasets: [{
                label: 'Air Quality Index (AQI)',
                data: [], // AQI values will be populated dynamically
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
            layout: {
                padding: 10
            },
            scales: {
                x: {
                    title: {
                        display: true,
                        text: 'Time (HH:MM)',
                        color: '#455a64',
                        font: {
                            size: 14,
                            weight: 'bold'
                        }
                    },
                    grid: {
                        color: 'rgba(189, 195, 199, 0.3)',
                        borderColor: '#b0bec5'
                    },
                    ticks: {
                        color: '#455a64',
                        font: { size: 12 }
                    }
                },
                y: {
                    beginAtZero: true,
                    max: 400,
                    title: {
                        display: true,
                        text: 'AQI Value',
                        color: '#455a64',
                        font: {
                            size: 14,
                            weight: 'bold'
                        }
                    },
                    grid: {
                        color: 'rgba(189, 195, 199, 0.3)',
                        borderColor: '#b0bec5'
                    },
                    ticks: {
                        color: '#455a64',
                        font: { size: 12 },
                        stepSize: 50
                    }
                }
            },
            plugins: {
                legend: {
                    display: true,
                    position: 'top',
                    labels: {
                        color: '#455a64',
                        font: {
                            size: 14,
                            weight: 'bold'
                        },
                        padding: 15
                    }
                },
                title: {
                    display: true,
                    text: 'Historical AQI Trends',
                    color: '#0288d1',
                    font: {
                        size: 18,
                        weight: 'bold'
                    },
                    padding: {
                        top: 10,
                        bottom: 20
                    }
                },
                tooltip: {
                    enabled: true,
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#ffffff',
                    bodyColor: '#ffffff',
                    borderColor: '#0288d1',
                    borderWidth: 1,
                    cornerRadius: 4,
                    padding: 10,
                    callbacks: {
                        label: function (context) {
                            return `AQI: ${context.parsed.y}`;
                        }
                    }
                }
            },
            animation: {
                duration: 1000,
                easing: 'easeInOutQuad'
            }
        }
    });


    // Sensor selection and chart update
    const sensorSelect = document.getElementById('sensorSelect');
    sensorSelect.addEventListener('change', function () {
        const selectedSensorId = this.value;
        if (selectedSensorId) {
            updateHistoricalChart(selectedSensorId);
        }
    });

    // Function to update historical chart (simulated data)
    function updateHistoricalChart(sensorId) {
        const sensor = sensors.find(s => s.id == sensorId);
        if (sensor) {
            const historicalData = Array(24).fill(0).map(() => Math.floor(Math.random() * 300)); // Simulated data
            const now = new Date(); // Current time
            const labels = Array(24).fill(0).map((_, i) => {
                const pastTime = new Date(now.getTime() - (23 - i) * 60 * 60 * 1000); // Subtract hours in milliseconds
                const hours = pastTime.getHours().toString().padStart(2, '0');
                const minutes = pastTime.getMinutes().toString().padStart(2, '0');
                return `${hours}:${minutes}`;
            });

            aqiChart.data.labels = labels;
            aqiChart.data.datasets[0].data = historicalData;
            aqiChart.update();
        }
    }

    // Show historical data from map popup
    window.showHistorical = function (sensorId) {
        sensorSelect.value = sensorId;
        updateHistoricalChart(sensorId);
    };

    // Admin: Add new sensor. DOUBLE CHECK THIS SEGMENT
    window.addSensor = function () {
        const sensorId = document.getElementById('sensorId').value;
        const locationName = document.getElementById('locationName').value;
        const lat = parseFloat(document.getElementById('lat').value);
        const lng = parseFloat(document.getElementById('lng').value);

        if (sensorId && locationName && !isNaN(lat) && !isNaN(lng)) {
            const newSensor = { id: sensorId, lat, lng, name: locationName, aqi: 0, active: true };
            sensors.push(newSensor);

            // Send to backend (example API call)
            fetch('/api/sensors/register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(newSensor)
            }).then(response => {
                if (response.ok) {
                    updateMap();
                    clearSensorForm();
                } else {
                    alert('Failed to register sensor');
                }
            });
        } else {
            alert('Please fill all fields correctly');
        }
    };

    function clearSensorForm() {
        document.getElementById('sensorId').value = '';
        document.getElementById('locationName').value = '';
        document.getElementById('lat').value = '';
        document.getElementById('lng').value = '';
    }

    // Admin: Save alert thresholds
    window.saveThresholds = function () {
        const moderateThreshold = parseInt(document.getElementById('moderateThreshold').value) || 51;
        const unhealthyThreshold = parseInt(document.getElementById('unhealthyThreshold').value) || 151;

        // Send to backend (example API call)
        // fetch('/api/alerts/thresholds', {
        //     method: 'POST',
        //     headers: { 'Content-Type': 'application/json' },
        //     body: JSON.stringify({ moderate: moderateThreshold, unhealthy: unhealthyThreshold })
        // }).then(response => {
        //     if (response.ok) {
        //         alert('Thresholds saved successfully');
        //         checkAlerts();
        //     } else {
        //         alert('Failed to save thresholds');
        //     }
        // });
    };

    // Check and display alerts
    function checkAlerts() {
        const alertDiv = document.getElementById('alertNotifications');
        if (!alertDiv) return;

        alertDiv.innerHTML = '';
        sensors.forEach(sensor => {
            if (sensor.active && sensor.aqi > 150) { // Example threshold (unhealthy)
                alertDiv.innerHTML += `<div style="color: #d32f2f;">${sensor.name}: AQI ${sensor.aqi} - Unhealthy</div>`;
            }
        });
    }
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
    setInterval(() => {
        updateMap();
        checkAlerts();
    }, 5000); // Refresh every 5 seconds
});