require 'bundler'
Bundler.setup :default, :build
require 'cocaine'
include Cocaine
require 'rake'
require 'rake/clean'
require 'albacore'
require 'pathname'
require './automation/logging.rb'
require './automation/teamcity.rb'
require './automation/clean.rb'
require './automation/assembly_info.rb'
require './automation/openwrap.rb'
require './automation/nunit.rb'
require './automation/ndepend.rb'

@log = setup_logging(ENV['verbosity'] || 'info')
configuration = ENV['msbuild_configuration'] || 'Release'
name = 'JustEat.Aop'
cmd_opts = {logger: @log}

setup_openwrap_dependencies
setup_nunit configuration: configuration
setup_ndepend configuration: configuration

desc 'Bootstrap all build-dependencies'
task :bootstrap => ['bootstrap:default', 'wrap:update']

desc "Compile solution"
msbuild :compile => [:assembly_info, 'wrap:update'] do |m|
	m.properties :configuration => configuration
	m.targets :Build
	m.solution = "#{name}.sln"
end
CLEAN.include 'bin', '**/obj'
CLOBBER.include 'wraps'

AssemblyInfoGenerator.new(log: @log).generate 
task :assembly_info => [:bootstrap]

namespace :wrap do
	directory 'bin/wraps'
	desc "Build #{name} wrap into bin/wraps"
	task :build => ['bin/wraps', 'bootstrap:openwrap'] do
		CommandLine.new('@o', 'build-wrap -Name JustEat.Bootstrapper -Path bin\\wraps -configuration release -incremental -quiet', cmd_opts).run
	end
	desc 'Update wraps from repositories.  This will pull in the latest versions available when a wrap is not present, and not locked.'
	task :update => ['bootstrap:openwrap'] do
		@log.happy "Updating wraps.  This can take some time (like, minutes)..."
		CommandLine.new(@o, 'update-wrap', cmd_opts).run
	end
	namespace :publish do
		[:continuous, :local].each do |remote|
			desc "publish to the '#{remote}' remote"
			task remote => ['bootstrap:openwrap', 'wrap:build'] do
				wrap = FileList.new('bin/wraps/*.wrap').sort.last
				@log.debug "Publishing #{wrap} to #{remote}"
				CommandLine.new(@o, "publish-wrap -remote #{remote.to_s} -path #{wrap.gsub('/','\\')}").run
			end
		end
	end
end

task :build => [:bootstrap, :assembly_info, :compile, :test, :analyse]
task :package => [:build, 'wrap:build', 'wrap:publish:local']
task :default => [:build]
