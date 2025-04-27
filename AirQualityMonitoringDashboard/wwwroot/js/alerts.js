// Air Quality Alerts System
document.addEventListener('DOMContentLoaded', function () {
    console.log('Alert system initializing...');

    // DOM Elements
    const notificationIcon = document.getElementById('notificationIcon');
    const notificationBadge = document.getElementById('notificationBadge');
    const notificationDropdown = document.getElementById('notificationDropdown');
    const alertNotifications = document.getElementById('alertNotifications');
    const clearAllButton = document.getElementById('clearAllAlerts');

    console.log('Notification icon found:', !!notificationIcon);
    console.log('Notification dropdown found:', !!notificationDropdown);
    console.log('Alert notifications container found:', !!alertNotifications);

    // Alert thresholds (can be customized)
    const alertThresholds = {
        good: 0,      // Added good (green)
        moderate: 51,     // Yellow
        unhealthySensitive: 101, // Orange
        unhealthy: 151,   // Red
        veryUnhealthy: 201, // Purple
        hazardous: 301    // Maroon
    };

    // Store active alerts
    let activeAlerts = [];
    let alertHistory = JSON.parse(localStorage.getItem('aqiAlertHistory') || '[]');

    // Initialize
    updateAlertBadge();

    // Toggle dropdown when clicking the notification icon
    if (notificationIcon) {
        notificationIcon.addEventListener('click', function (e) {
            console.log('Notification icon clicked');
            e.stopPropagation(); // Prevent immediate closure
            notificationDropdown.classList.toggle('show');
            console.log('Dropdown visibility:', notificationDropdown.classList.contains('show'));
        });
    }

    // Close dropdown when clicking outside
    document.addEventListener('click', function (event) {
        if (notificationDropdown && notificationDropdown.classList.contains('show') &&
            !notificationIcon.contains(event.target) &&
            !notificationDropdown.contains(event.target)) {
            notificationDropdown.classList.remove('show');
            console.log('Dropdown closed by outside click');
        }
    });

    // Clear all alerts
    if (clearAllButton) {
        clearAllButton.addEventListener('click', function (e) {
            e.stopPropagation(); // Prevent dropdown closure
            console.log('Clear all alerts clicked');
            activeAlerts = [];
            updateAlertUI();
            updateAlertBadge();
        });
    }

    // Get AQI category and severity
    function getAQICategory(aqi) {
        if (aqi <= 50) return { category: 'Good', severity: 'good' };
        if (aqi <= 100) return { category: 'Moderate', severity: 'moderate' };
        if (aqi <= 150) return { category: 'Unhealthy for Sensitive Groups', severity: 'unhealthy-sensitive' };
        if (aqi <= 200) return { category: 'Unhealthy', severity: 'unhealthy' };
        if (aqi <= 300) return { category: 'Very Unhealthy', severity: 'very-unhealthy' };
        return { category: 'Hazardous', severity: 'hazardous' };
    }

    // Check for air quality alerts
    function checkAirQualityAlerts(sensors) {
        console.log('Checking AQI alerts for', sensors?.length || 0, 'sensors');
        if (!sensors || sensors.length === 0) {
            console.log('No sensors available');
            return;
        }

        let newAlerts = false;

        sensors.forEach(sensor => {
            if (!sensor.active) return;

            const { category, severity } = getAQICategory(sensor.aqi);
            console.log(`Sensor ${sensor.name || sensor.id}: AQI ${sensor.aqi}, Category: ${category}`);

            // IMPORTANT: No longer skip "good" severity
            // We want to include all alerts, even "good" ones

            // Check if alert already exists for this sensor
            const existingAlertIndex = activeAlerts.findIndex(alert => alert.sensorId === sensor.id);

            if (existingAlertIndex >= 0) {
                // Update existing alert if severity changed
                if (activeAlerts[existingAlertIndex].severity !== severity) {
                    console.log(`Updating alert for sensor ${sensor.name || sensor.id} - severity changed to ${severity}`);
                    activeAlerts[existingAlertIndex] = {
                        sensorId: sensor.id,
                        location: sensor.name || sensor.location || `Sensor ${sensor.id}`,
                        aqi: sensor.aqi,
                        category: category,
                        severity: severity,
                        time: new Date(),
                        message: createAlertMessage(category, sensor.aqi)
                    };
                    newAlerts = true;
                }
            } else {
                // Add new alert
                console.log(`Creating new alert for sensor ${sensor.name || sensor.id} - ${severity}`);
                activeAlerts.push({
                    sensorId: sensor.id,
                    location: sensor.name || sensor.location || `Sensor ${sensor.id}`,
                    aqi: sensor.aqi,
                    category: category,
                    severity: severity,
                    time: new Date(),
                    message: createAlertMessage(category, sensor.aqi)
                });
                newAlerts = true;
            }
        });

        // Update UI if we have new alerts
        if (newAlerts) {
            console.log('New alerts detected - updating UI');
            if (notificationIcon) {
                notificationIcon.classList.add('pulse');
                setTimeout(() => {
                    notificationIcon.classList.remove('pulse');
                }, 1000);
            }

            updateAlertUI();
            updateAlertBadge();
        }

        // Always update UI even if no new alerts (in case this is first run)
        if (activeAlerts.length > 0 && alertNotifications.children.length <= 1) {
            console.log('Forcing UI update for existing alerts');
            updateAlertUI();
            updateAlertBadge();
        }
    }

    // Create a message based on the alert category
    function createAlertMessage(category, aqi) {
        switch (category.toLowerCase()) {
            case 'good':
                return `Good air quality: AQI is ${aqi}`;
            case 'moderate':
                return `Moderate air quality alert: AQI is ${aqi}`;
            case 'unhealthy for sensitive groups':
                return `Unhealthy for sensitive groups: AQI is ${aqi}`;
            case 'unhealthy':
                return `Unhealthy air quality: AQI is ${aqi}`;
            case 'very unhealthy':
                return `Very unhealthy air quality: AQI is ${aqi}`;
            case 'hazardous':
                return `Hazardous air quality: AQI is ${aqi}`;
            default:
                return `AQI is ${aqi} (${category})`;
        }
    }

    // Update alert badge
    function updateAlertBadge() {
        const count = activeAlerts.length;
        console.log('Updating alert badge count:', count);

        if (notificationBadge) {
            notificationBadge.textContent = count;
            if (count > 0) {
                notificationBadge.classList.add('active');
            } else {
                notificationBadge.classList.remove('active');
            }
        }
    }

    // Update alert UI
    function updateAlertUI() {
        if (!alertNotifications) {
            console.error('Alert notifications container not found');
            return;
        }

        console.log('Updating alert UI with', activeAlerts.length, 'alerts');

        if (activeAlerts.length === 0) {
            alertNotifications.innerHTML = '<div class="empty-notification">No active alerts</div>';
            return;
        }

        alertNotifications.innerHTML = '';

        // Sort alerts by severity (hazardous first, good last)
        const sortedAlerts = [...activeAlerts].sort((a, b) => {
            const severityOrder = {
                'hazardous': 6,
                'very-unhealthy': 5,
                'unhealthy': 4,
                'unhealthy-sensitive': 3,
                'moderate': 2,
                'good': 1
            };
            return severityOrder[b.severity] - severityOrder[a.severity];
        });

        sortedAlerts.forEach((alert, index) => {
            const timeStr = formatTime(alert.time);
            const alertItem = document.createElement('div');
            alertItem.className = `alert-item alert-${alert.severity}`;
            alertItem.innerHTML = `
                <div class="alert-info">
                    <div class="alert-location">${alert.location}</div>
                    <div class="alert-message">${alert.message}</div>
                    <div class="alert-time">${timeStr}</div>
                </div>
                <button class="alert-close" data-index="${index}">×</button>
            `;
            alertNotifications.appendChild(alertItem);
        });

        // Add close button functionality
        document.querySelectorAll('.alert-close').forEach(button => {
            button.addEventListener('click', function (e) {
                e.stopPropagation();
                const index = parseInt(this.getAttribute('data-index'));
                dismissAlert(index);
            });
        });
    }

    // Dismiss a specific alert
    function dismissAlert(index) {
        console.log('Dismissing alert at index', index);
        if (index >= 0 && index < activeAlerts.length) {
            // Add to history before removing
            addToAlertHistory(activeAlerts[index]);
            activeAlerts.splice(index, 1);
            updateAlertUI();
            updateAlertBadge();
        }
    }

    // Add to alert history
    function addToAlertHistory(alert) {
        const historyItem = {
            ...alert,
            dismissedAt: new Date()
        };
        alertHistory.unshift(historyItem);

        // Keep history limited to 50 items
        if (alertHistory.length > 50) {
            alertHistory.pop();
        }

        // Store in localStorage
        localStorage.setItem('aqiAlertHistory', JSON.stringify(alertHistory));
    }

    // Format time for display
    function formatTime(date) {
        if (typeof date === 'string') {
            date = new Date(date);
        }

        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);

        if (diffMins < 1) return 'Just now';
        if (diffMins < 60) return `${diffMins} minutes ago`;

        const hours = date.getHours().toString().padStart(2, '0');
        const mins = date.getMinutes().toString().padStart(2, '0');
        return `Today at ${hours}:${mins}`;
    }

    // Fetch alerts from API when available
    async function fetchAlertsFromAPI() {
        try {
            console.log('Fetching alerts from API...');
            const response = await fetch('/api/Alert');
            if (response.ok) {
                const apiAlerts = await response.json();
                console.log('Received', apiAlerts.length, 'alerts from API');

                // Clear existing alerts and add from API
                activeAlerts = [];

                // Process API alerts
                apiAlerts.forEach(alert => {
                    // Only add active alerts
                    if (alert.isActive) {
                        activeAlerts.push({
                            id: alert.id,
                            sensorId: alert.sensorId,
                            location: alert.sensorName || alert.sensorLocation || 'Unknown Location',
                            aqi: alert.aqi,
                            category: alert.category,
                            severity: alert.severity,
                            time: new Date(alert.createdAt),
                            message: alert.message
                        });
                    }
                });

                updateAlertUI();
                updateAlertBadge();
                return true;
            } else {
                console.warn('API returned status:', response.status);
                // Fall back to generating alerts from sensor data
                generateClientSideAlerts();
                return false;
            }
        } catch (error) {
            console.error('Error fetching alerts from API:', error);
            // Fall back to generating alerts from sensor data
            generateClientSideAlerts();
            return false;
        }
    }

    // Create test alerts via API
    async function createTestAlertsViaAPI() {
        try {
            console.log('Creating test alerts via API...');
            const response = await fetch('/api/Alert/test', {
                method: 'POST'
            });

            if (response.ok) {
                console.log('Test alerts created successfully');
                await fetchAlertsFromAPI();
                return true;
            } else {
                console.warn('API returned status:', response.status);
                return false;
            }
        } catch (error) {
            console.error('Error creating test alerts:', error);
            return false;
        }
    }

    // Generate client-side alerts when API fails
    function generateClientSideAlerts() {
        console.log('Generating client-side alerts from sensor data');
        if (window.aqiSensorsData && Array.isArray(window.aqiSensorsData)) {
            checkAirQualityAlerts(window.aqiSensorsData);
        } else {
            // Create test alerts for debugging
            console.log('Creating test alerts for debugging');
            createTestAlerts();
        }
    }

    // Create test alerts for testing purposes
    function createTestAlerts() {
        console.log('Creating test alerts');

        // Clear existing alerts
        activeAlerts = [];

        // Add test alerts for different categories
        activeAlerts.push({
            sensorId: 1,
            location: 'Test Sensor 1',
            aqi: 30,
            category: 'Good',
            severity: 'good',
            time: new Date(),
            message: 'Good air quality: AQI is 30'
        });

        activeAlerts.push({
            sensorId: 2,
            location: 'Test Sensor 2',
            aqi: 75,
            category: 'Moderate',
            severity: 'moderate',
            time: new Date(),
            message: 'Moderate air quality alert: AQI is 75'
        });

        activeAlerts.push({
            sensorId: 3,
            location: 'Test Sensor 3',
            aqi: 125,
            category: 'Unhealthy for Sensitive Groups',
            severity: 'unhealthy-sensitive',
            time: new Date(),
            message: 'Unhealthy for sensitive groups: AQI is 125'
        });

        updateAlertUI();
        updateAlertBadge();
    }

    // Hook into the existing sensor data updates
    // Check for sensors update every 5 seconds (this will work with the simulated data)
    const checkSensorsInterval = setInterval(() => {
        if (window.aqiSensorsData && Array.isArray(window.aqiSensorsData)) {
            console.log('Found sensor data in window object:', window.aqiSensorsData.length, 'sensors');
            checkAirQualityAlerts(window.aqiSensorsData);
        }
    }, 5000);

    // Expose functions to window for integration with dashboard.js
    window.aqiAlertSystem = {
        checkAlerts: checkAirQualityAlerts,
        addAlert: function (sensor) {
            console.log('Manual alert addition for sensor:', sensor);
            const { category, severity } = getAQICategory(sensor.aqi);

            // IMPORTANT: No longer skip "good" severity

            // Create message based on severity
            const message = createAlertMessage(category, sensor.aqi);

            activeAlerts.push({
                sensorId: sensor.id,
                location: sensor.name || sensor.location || `Sensor ${sensor.id}`,
                aqi: sensor.aqi,
                category: category,
                severity: severity,
                time: new Date(),
                message: message
            });

            updateAlertUI();
            updateAlertBadge();
        },
        createTestAlerts: createTestAlerts
    };

    // Try to fetch alerts from API initially and then create test alerts if needed
    fetchAlertsFromAPI().then(success => {
        if (!success || activeAlerts.length === 0) {
            // Try to create test alerts via API
            createTestAlertsViaAPI().then(testSuccess => {
                if (!testSuccess) {
                    // Fall back to client-side test alerts
                    createTestAlerts();
                }
            });
        }
    });

    // Periodically refresh alerts from API
    setInterval(fetchAlertsFromAPI, 60000); // every minute

    // Make sure notification dropdown is working
    console.log('Alert system initialization complete');
});