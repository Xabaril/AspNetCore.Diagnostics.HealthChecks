import React, { FunctionComponent } from 'react';
import MenuIcon from '../../assets/svg/menu.svg';

interface AsideMenuProps {
  onClick: () => void;
  children: any;
  isOpen: boolean;
}

const AsideMenu : FunctionComponent<AsideMenuProps> = (props: AsideMenuProps) => {
  return (
    <aside className={`hc-aside ${props.isOpen ? 'is-open' : ''}`}>
      <button
        title={props.isOpen ? 'close menu' : 'open menu'}
        className="hc-aside__open-btn"
        onClick={() => {
          props.onClick();
        }}>
        <i className="material-icons">{props.isOpen ? 'menu_open' : 'menu'}</i>
      </button>
      <div className="hc-aside__logo" title="Logo as background image" />
      <nav className="hc-aside-menu">{props.children}</nav>
    </aside>
  );
};

export { AsideMenu };
