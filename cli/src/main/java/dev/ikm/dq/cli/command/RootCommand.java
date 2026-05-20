package dev.ikm.dq.cli.command;

import org.springframework.stereotype.Component;
import picocli.CommandLine;

@Component
@CommandLine.Command(name = "data-quality",
		description = "Data Quality System CLI",
		mixinStandardHelpOptions = true)
public class RootCommand implements Runnable {

	@Override
	public void run() {
		System.out.println("Use a subcommand. Try --help");
	}
}
