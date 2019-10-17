import React from 'react';

interface IDetailPanelProps {
  children: React.ReactNode;
  onClosePanel: any;
}

const LivenessPanel = ({
  children,
  onClosePanel
}: IDetailPanelProps) => (
  <aside
    className="hc-liveness-panel">
    <button className="hc-action-btn" onClick={() => onClosePanel()}>
      <i className="material-icons">close</i>
    </button>
    {children}
  </aside>
);

export { LivenessPanel };
