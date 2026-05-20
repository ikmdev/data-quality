package dev.ikm.dq.cli.config;

import dev.ikm.dq.cli.command.BackupCommand;
import dev.ikm.dq.cli.command.EvaluateCommand;
import dev.ikm.dq.cli.command.IngestCommand;
import dev.ikm.dq.cli.command.PipelineCommand;
import dev.ikm.dq.cli.command.RootCommand;
import org.springframework.boot.ApplicationArguments;
import org.springframework.boot.CommandLineRunner;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import picocli.CommandLine;

@Configuration
public class CommandRunnerConfig {

	@Bean
	CommandLineRunner commandLineRunner(ApplicationArguments appArgs,
	                                    RootCommand rootCommand,
	                                    IngestCommand ingestCommand,
	                                    EvaluateCommand evaluateCommand,
	                                    BackupCommand backupCommand,
										PipelineCommand pipelineCommand) {
		return args -> {
			int exitCode = new CommandLine(rootCommand)
					.addSubcommand(ingestCommand)
					.addSubcommand(evaluateCommand)
					.addSubcommand(backupCommand)
					.addSubcommand(pipelineCommand)
					.execute(appArgs.getSourceArgs());
			if (exitCode != 0) {
				System.exit(exitCode);
			}
		};
	}
}
