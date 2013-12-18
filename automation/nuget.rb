def setup_nuget opts={}
	name = opts[:name]
	configuration = opts[:configuration]
	version = opts[:version] || version
	nuget = opts[:nuget_exe] || ".nuget/nuget.exe"

	namespace :nuget do
		directory 'packages'
		desc "restore packages"
		task :restore => ['packages'] do
			FileList.new("**/packages.config").map{|pc|Pathname.new(pc)}.each do |pc|
				CommandLine.new(nuget, "install \"#{pc.to_s.gsub('/', '\\')}\" -source http://packages.je-labs.com/nuget/Default/ -source http://nuget.org/api/v2/ -o packages", logger: Logger.new(STDOUT)).run
			end
		end

		desc "Harvest the output to prepare package"
		task :harvest

		package_dir = "out/package"
		package_lib = "#{package_dir}/lib"
		directory package_dir
		directory package_lib

		task :harvest => [package_lib] do
			lib_files = FileList.new("out/**/#{configuration}/*.{exe,config,dll,pdb,xml}")
			lib_files.exclude /(Shouldly|Rhino|nunit|Test|NLog|Ploeh)/
			lib_files.map{|f|Pathname.new f}.each do |f|
				harvested = "#{package_lib}/#{f.basename}"
				FileUtils.cp_r f, harvested
			end
		end

		desc "Create the nuspec"
		nuspec :nuspec => [package_dir, :harvest] do |nuspec|
			nuspec.id = name
			nuspec.version = version
			nuspec.authors = "Peter Mounce"
			nuspec.owners = "Peter Mounce"
			nuspec.description = "JustEat.StatsD is our library for interacting with StatsD event-tracking"
			nuspec.summary = "JustEat.StatsD provides helpers for publishing events into statsd"
			nuspec.language = "en-GB"
			nuspec.licenseUrl = "https://github.je-labs.com/PaaS/#{name}/blob/master/LICENSE.txt"
			nuspec.projectUrl = "https://github.je-labs.com/PaaS/#{name}"
			nuspec.working_directory = package_dir
			nuspec.output_file = "#{name}.nuspec"
			nuspec.tags = "justeat library events statsd paas"
      nuspec.dependency "NLog", "2.1.0"
		end

		nupkg = "out/#{name}.#{version}.nupkg"
		desc "Create the nuget package"
		file nupkg => [:nuspec] do |nugetpack|
			CommandLine.new(nuget, "pack out\\package\\#{name}.nuspec -basepath out\\package -o out").run
		end
		task :build => nupkg

		task :default => [:harvest, :nuspec, :build]
	end
	task :nuget => 'nuget:default'
end
