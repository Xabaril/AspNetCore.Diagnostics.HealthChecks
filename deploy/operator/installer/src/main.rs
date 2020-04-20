#[macro_use]
extern crate serde_derive;
mod tree;
use crate::tree::{Tree, TreeDefinition};
use async_std::task;
use crossterm::{
    execute,
    style::{Color, Print, ResetColor, SetForegroundColor},
};

use std::io::{stdout, Write};
use std::process::{exit, Command};

const API_HC_REPOSITORY: &str =
    "https://api.github.com/repos/Xabaril/AspNetCore.Diagnostics.HealthChecks";
const RAW_HC_REPOSITORY: &str =
    "https://raw.githubusercontent.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/master";
const OPERATOR_CRD_PATH: &str = "deploy/operator/crd/healthcheck-crd.yaml";
const OPERATOR_TREE: &str = "31bb25e1778dd3af99501ca457d0ef653ea31618";
const USER_AGENT: &str = "Operator Installer Agent";

#[derive(Debug, PartialEq)]
enum K8sCommand {
    Apply,
    Delete,
}

impl std::fmt::Display for K8sCommand {
    fn fmt(&self, f: &mut std::fmt::Formatter) -> std::fmt::Result {
        write!(f, "{:?}", self)
    }
}

fn main() -> Result<(), Box<dyn std::error::Error + Send + Sync + 'static>> {
    let args: Vec<String> = std::env::args().collect();

    let mut k8s_command = K8sCommand::Apply;

    if args.len() > 1 {
        k8s_command = match args[1].as_str() {
            "--delete" => K8sCommand::Delete,
            _ => k8s_command,
        };
    }

    task::block_on(async {
        display(Color::Cyan, "Starting HealthChecks operator installer");

        check_kubectl();

        display(
            Color::Green,
            &format!("Executing {} in all operator resources", k8s_command),
        );
        execute_crd(&k8s_command);

        display(
            Color::Magenta,
            "Reading yaml definition files from healthchecks repository",
        );

        let tree_url = format!("{}/git/trees/{}", API_HC_REPOSITORY, OPERATOR_TREE);

        let client = surf::Client::new();
        let resp: Tree = client
            .get(tree_url)
            .set_header("USER-AGENT", USER_AGENT)
            .recv_json()
            .await?;

        let mut blobs = resp
            .tree
            .into_iter()
            .filter(|t| t.file_type == "blob")
            .collect::<Vec<TreeDefinition>>();

        blobs.sort_by(|a, b| a.path.cmp(&b.path));

        for definition in blobs {
            let definition_path = format!(
                "{}/{}/{}",
                RAW_HC_REPOSITORY, "deploy/operator", definition.path
            );
            if definition_path.contains("namespace") && k8s_command == K8sCommand::Delete {
                continue;
            }

            display(Color::Green, &format!("Processing {}", definition.path));
            execute_definition(definition_path, &k8s_command);
        }

        display_deploy();
        display(
            Color::Green,
            &format!("Healthchecks Operator {} finished", k8s_command),
        );
        Ok(())
    })
}

fn check_kubectl() {
    if Command::new("kubectl").output().is_err() {
        display(
            Color::Red,
            "kubectl is not installed. Please install kubectl cli",
        );
        exit(1);
    }
}

fn execute_crd(command: &K8sCommand) {
    display(Color::Green, "Processing Custom Resource Definition");
    let crd_path = format!("{}/{}", RAW_HC_REPOSITORY, OPERATOR_CRD_PATH);
    execute_definition(crd_path, &command)
}

fn execute_definition(definition_path: String, command: &K8sCommand) {
    let process = Command::new("kubectl")
        .args(&[&command.to_string().to_lowercase(), "-f", &definition_path])
        .output()
        .unwrap();
    if !process.status.success() {
        display(Color::Red, &String::from_utf8_lossy(&process.stderr));
        exit(1);
    }

    display(Color::White, &String::from_utf8_lossy(&process.stdout));
}

fn display_deploy() {
    display(
        Color::White,
        &String::from_utf8_lossy(
            &Command::new("kubectl")
                .args(&["get", "deploy", "-n", "healthchecks"])
                .output()
                .unwrap()
                .stdout,
        ),
    )
}

fn display(color: Color, text: &str) {
    execute!(
        stdout(),
        SetForegroundColor(color),
        Print(format!("{}\n", text)),
        ResetColor
    )
    .unwrap();
}
