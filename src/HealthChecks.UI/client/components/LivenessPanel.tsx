import React from 'react';
import classNames from 'classnames';

interface IDetailPanelProps {
  isOpen: boolean;
  children: React.ReactNode;
  onClosePanel: any;
}

const LivenessPanel = ({
  isOpen,
  children,
  onClosePanel
}: IDetailPanelProps) => (
  <aside
    className={classNames('hc-liveness-panel', {
      'hc-liveness-panel--open': isOpen
    })}>
    <button className="hc-action-btn" onClick={() => onClosePanel()}>
      <i className="material-icons">close</i>
    </button>
    {children}
  </aside>
);

export { LivenessPanel };
