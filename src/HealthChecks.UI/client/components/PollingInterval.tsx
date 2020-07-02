import * as React from "react";

export interface PollingIntervalState {
    active: number;
    editing: string;
    canSave: boolean;
}

export interface PollingIntervalProps {
    onChange(): void;
}

const healthChecksIntervalStorageKey = "healthchecks-ui-polling";

function validateInterval(raw: string | number | null | undefined) {
    if (raw !== null && raw !== undefined) {
        const numeric = typeof raw === "number" ? raw : parseInt(raw, 10);
        if (!isNaN(numeric)) {
            return {
                valid: numeric >= clientOptions.minimumPollingIntervalSeconds,
                normalized: Math.max(numeric, clientOptions.minimumPollingIntervalSeconds)
            };
        }
    }

    return {
        valid: false,
        normalized: Math.max(clientOptions.defaultPollingIntervalSeconds,
            clientOptions.minimumPollingIntervalSeconds)
    };
}

export function getConfiguredInterval() {
    return clientOptions.hidePollingIntervalControl
        ? clientOptions.defaultPollingIntervalSeconds
        : validateInterval(localStorage.getItem(healthChecksIntervalStorageKey)).normalized;
}

export class PollingInterval extends React.Component<PollingIntervalProps, PollingIntervalState> {

    constructor(props: PollingIntervalProps) {
        super(props);

        const active = getConfiguredInterval();

        this.state = {
            active,
            editing: active + "",
            canSave: false
        }
    }

    configurePolling = () => {
        if (!this.state.canSave) {
            return;
        }

        const newInterval = validateInterval(this.state.editing).normalized;
        const newInput = newInterval + "";

        this.setState({
            active: newInterval,
            editing: newInput,
            canSave: false,
        })

        localStorage.setItem(healthChecksIntervalStorageKey, newInput);
        this.props.onChange();
    }

    onPollingIntervalChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const validation = validateInterval(event.target.value);
        this.setState({
            editing: event.target.value,
            canSave: validation.valid &&
                validation.normalized !== this.state.active
        });
    }

    render() {
        return (
            clientOptions.hidePollingIntervalControl ? <div /> : (
                <>
                    <label>Refresh every</label>
                    <input
                        value={this.state.editing}
                        onChange={this.onPollingIntervalChange}
                        type="number"
                        data-oninput="validity.valid && value > 0 ||(value=10)"
                        title={`Miminum value: ${clientOptions.minimumPollingIntervalSeconds}`}
                    />
                    <label>seconds</label>
                    <button
                        onClick={this.configurePolling}
                        type="button"
                        className="hc-button"                        
                        disabled={!this.state.canSave}
                    >Change</button>
                </>
            )
        );
    }
}