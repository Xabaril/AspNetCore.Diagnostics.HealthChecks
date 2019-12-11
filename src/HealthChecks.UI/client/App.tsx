import React from 'react';
import { Route, Redirect, NavLink } from 'react-router-dom';
import { LivenessPage } from './components/LivenessPage';
import { WebhooksPage } from './components/WebhooksPage';
import { AsideMenu } from './components/AsideMenu';

interface AppProps {
  apiEndpoint: string;
  webhookEndpoint: string;
  asideMenuOpened: boolean;
}

interface AppState {
  menuOpen: boolean;
}

const WhiteGearIcon = require('../assets/svg/white-gear.svg');
const WhiteHeartIcon = require('../assets/svg/heart-check.svg');
const SelectedHeartIcon = require('../assets/svg/heart-check.svg');

export class App extends React.Component<AppProps, AppState> {
  constructor(props: AppProps) {
    super(props);
    this.state = {
      menuOpen: props.asideMenuOpened
    };

    this.toggleMenu = this.toggleMenu.bind(this);
  }

  toggleMenu() {
    this.setState({
      menuOpen: !this.state.menuOpen
    });
  }
  render() {
    return (
      <main id="outer-container">
        <AsideMenu
          isOpen={this.state.menuOpen}
          onClick={() => this.setState({ menuOpen: !this.state.menuOpen })}>
          <NavLink
            to="/healthchecks"
            className="hc-aside-menu__item"
            activeClassName="hc-aside-menu__item--active"
            >
            <img alt="icon heart check" className="hc-menu-icon" src={WhiteHeartIcon} />
            <span>Health Checks</span>
          </NavLink>
          <NavLink
            to="/webhooks"
            className="hc-aside-menu__item"
            activeClassName="hc-aside-menu__item--active"
            >
            <img alt="icon gear" className="hc-menu-icon" src={WhiteGearIcon} />
            <span>Webhooks</span>
          </NavLink>
        </AsideMenu>
        <section className="hc-section-router">
          <Route
            exact
            path="/"
            render={() => <Redirect to="/healthchecks" />}
          />
          <Route
            path="/healthchecks"
            render={() => <LivenessPage endpoint={this.props.apiEndpoint} />}
          />
          <Route
            path="/webhooks"
            render={() => (
              <WebhooksPage endpoint={this.props.webhookEndpoint} />
            )}
          />
        </section>
      </main>
    );
  }
}
