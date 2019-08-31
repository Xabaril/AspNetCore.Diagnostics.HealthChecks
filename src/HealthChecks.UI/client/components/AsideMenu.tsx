import React from 'react';
const MenuIcon = require('../../assets/svg/menu.svg');
interface AsideMenuProps {
  onClick: () => void;
  children: any;
  isOpen: boolean;
}

const AsideMenu = (props: AsideMenuProps) => {
  return (
    <aside className={`hc-aside ${props.isOpen ? 'hc-aside--open' : ''}`}>
      <button
        className="hc-aside__open-btn"
        onClick={() => {
          props.onClick();
        }}>
        <i className="material-icons">{props.isOpen ? 'menu_open' : 'menu'}</i>
      </button>
      <div className="hc-aside__logo" />
      <nav className="hc-aside-menu">{props.children}</nav>
    </aside>
  );
};

export { AsideMenu };
