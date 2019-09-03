export const statusUp: string = 'Up',
  statusDown: string = 'Down',
  statusDegraded: string = 'Degraded';

const kubernetesIcon = require('../assets/images/kubernetes-icon.png');

const imageResources = [
  { state: 'Failed', image: 'error', color: '--dangerColor' },
  { state: 'Unhealthy', image: 'error', color: '--dangerColor' },
  { state: 'Degraded', image: 'warning', color: '--warningColor' },
  { state: 'Healthy', image: 'check_circle', color: '--successColor' }
];

export const discoveryServices = [
  { name: 'kubernetes', image: kubernetesIcon }
];

const getStatusConfig = (status: string) =>
  imageResources.find(s => s.state == status);

export { getStatusConfig };
