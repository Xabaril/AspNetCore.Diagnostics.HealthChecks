import React, { FunctionComponent } from 'react';

interface IDetailPanelProps {
  children: React.ReactNode;
  onClosePanel: any;
  visible: boolean
}

const LivenessPanel: FunctionComponent<IDetailPanelProps> = ({
  children,
  visible,
  onClosePanel
}) => (
  <aside style={{right: visible ? "0": "-32rem"}}
    className="hc-liveness-panel">
    <button className="hc-action-btn" onClick={() => onClosePanel()}>
      <i className="material-icons">close</i>
    </button>
    {children}
  </aside>
);

export { LivenessPanel };
